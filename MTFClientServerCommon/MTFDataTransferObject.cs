//MTFSequence line 199 - method replace GUIDs must be changed as is in comments

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFObservableCollection<T> : ObservableCollection<T>
        where T : MTFDataTransferObject
    {
        public MTFObservableCollection()
            : base()
        {
        }

        public MTFObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public MTFObservableCollection(IList<T> list)
            : base(list)
        {
        }

        public object Clone()
        {
            return this;
        }

        public void Add(object item)
        {
            if (item is T)
            {
                base.Add((T)item);
            }
        }

        public object Copy()
        {
            return this;
        }
    }

    [Serializable]
    public abstract class MTFDataTransferObject : ISerializable, INotifyPropertyChanged, ICloneable, IIdentifier
    {
        protected Dictionary<string, object> properties = new Dictionary<string, object>();
        public static Func<Guid, MTFDataTransferObject> LazyLoadCallBack;

        public static Dictionary<Guid, MTFDataTransferObject> LazyLoadCache = new Dictionary<Guid, MTFDataTransferObject>();
        public static bool RaiseNotifyPropertyChangedEvent = true;
        private bool isModified;

        protected MTFDataTransferObject()
        {
            SetProperty(Guid.NewGuid(), nameof(Id));
            SetProperty(ObjectVersion, nameof(ObjectVersion));
        }

        protected MTFDataTransferObject(SerializationInfo info, StreamingContext context)
        {
            try
            {
                SerializationInfoEnumerator valEnum = info.GetEnumerator();
                valEnum.MoveNext();

                MemoryStream ms;
                IFormatter formatter = new BinaryFormatter();
                if (valEnum.Name == "properties")
                {
                    ms = new MemoryStream((byte[])valEnum.Value);
                    var data = formatter.Deserialize(ms);
                    properties = (Dictionary<string, object>)data;
                    if (RaiseNotifyPropertyChangedEvent)
                    {
                        foreach (string propertyName in properties.Keys)
                        {
                            if (!(properties[propertyName] is MTFIdentityObject))
                            {
                                AdoptMember(properties[propertyName], propertyName);
                            }
                        }
                    }
                    ms.Close();
                    ms.Dispose();
                }
                if (!properties.ContainsKey(nameof(ObjectVersion)))
                {
                    SetProperty("0.0.0", nameof(ObjectVersion));
                }

                if (((string)properties[nameof(ObjectVersion)]).CompareTo(ObjectVersion) != 0)
                {
                    VersionConvert(GetProperty<string>(nameof(ObjectVersion)));
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage($"Deserialization of object {this.GetType().FullName} raised exception. See bellow.");
                SystemLog.LogException(e);
            }
        }

        public static void RemoveFromLazyLoadCache(Guid id)
        {
            if (LazyLoadCache.ContainsKey(id))
            {
                LazyLoadCache.Remove(id);
            }
        }

        public Dictionary<string, object> InternalProperties => properties;

        public virtual string ObjectVersion => "1.0.0";

        public Guid Id
        {
            get => GetProperty<Guid>();
            set => SetProperty(value);
        }

        public bool IsLazyLoad
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IsModified
        {
            get => isModified;
            set
            {
                if (UseIsModified)
                {
                    isModified = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected static void ExecuteWithoutSetIsModified(Action action)
        {
            var useIsModified = UseIsModified;
            UseIsModified = false;

            action();

            UseIsModified = useIsModified;
        }

        private static bool useIsModified = true;
        private static bool UseIsModified
        {
            get => useIsModified;
            set => useIsModified = value;
        }

        public bool IsLoaded
        {
            get;
            set;
        }

        public T GetParent<T>() where T : MTFDataTransferObject
        {
            if (Parent == null)
            {
                return null;
            }

            if (Parent is T)
            {
                return (T)Parent;
            }

            return Parent.GetParent<T>();
        }

        public MTFDataTransferObject Parent
        {
            get;
            set;
        }

        protected virtual void VersionConvert(string fromVersion)
        {
            SetProperty(ObjectVersion, nameof(ObjectVersion));
        }

        protected virtual void SetProperty(object value, [CallerMemberName] string propertyName = null)
        {
            SetProperty(value, true, propertyName);
        }

        protected virtual void SetProperty(object value, bool adoptMember, [CallerMemberName] string propertyName = null)
        {
            bool valueChanged = true;
            if (properties.ContainsKey(propertyName))
            {
                valueChanged = value != properties[propertyName];
            }

            properties[propertyName] = value;

            if (valueChanged && RaiseNotifyPropertyChangedEvent)
            {
                if (adoptMember)
                {
                    AdoptMember(value, propertyName);
                }
                NotifyPropertyChanged(propertyName);
            }
        }

        protected virtual T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            CheckLazyLoad(propertyName);
            if (properties.ContainsKey(propertyName))
            {
                return (T)properties[propertyName];
            }

            return default(T);
        }

        private void CheckLazyLoad(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || propertyName == "Id" || propertyName == "IsLazyLoad")
            {
                return;
            }
            if (IsLazyLoad && !IsLoaded && properties.ContainsKey("IsLazyLoad") && (bool)properties["IsLazyLoad"])
            {
                if (LazyLoadCallBack == null)
                {
                    throw new Exception("No callback for lazy load is defined.");
                }

                var dto = LazyLoadCallBack(Id);
                if (dto != null)
                {
                    this.properties = dto.properties;
                    IsLoaded = true;
                }
            }
        }

        public void ChangeId(Guid oldId, Guid newId)
        {
            List<MTFDataTransferObject> objToModify = new List<MTFDataTransferObject>();
            foreachProperty((propertyName, dataObject) =>
            {
                if (propertyName == nameof(Id) && dataObject.Id == oldId)
                {
                    objToModify.Add(dataObject);
                }
            });
            objToModify.ForEach(obj => obj.Id = newId);
        }

        private void foreachProperty(Action<string, MTFDataTransferObject> func)
        {
            var propertyPath = new List<Guid>();
            foreachProperty(func, ref propertyPath);
        }

        private void foreachProperty(Action<string, MTFDataTransferObject> func, ref List<Guid> propertyPath)
        {
            foreach (string propertyName in properties.Keys)
            {
                if (properties[propertyName] is MTFDataTransferObject)
                {
                    //detect cycle
                    if (!propertyPath.Contains(this.Id))
                    {
                        propertyPath.Add(this.Id);
                        func(propertyName, this);
                        ((MTFDataTransferObject)properties[propertyName]).foreachProperty(func, ref propertyPath);
                        propertyPath.Remove(this.Id);
                    }
                }
                else if (properties[propertyName] is IEnumerable<MTFDataTransferObject>)
                {
                    foreach (MTFDataTransferObject dataTransferObj in ((IEnumerable<MTFDataTransferObject>)properties[propertyName]))
                    {
                        //detect cycle
                        if (dataTransferObj != null && !propertyPath.Contains(this.Id))
                        {
                            propertyPath.Add(this.Id);
                            func(propertyName, this);
                            dataTransferObj.foreachProperty(func, ref propertyPath);
                            propertyPath.Remove(this.Id);
                        }
                    }
                }
                else
                {
                    func(propertyName, this);
                }
            }
        }

        protected virtual void BeforeGetObjectData()
        { }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            BeforeGetObjectData();

            MemoryStream ms = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();

            //TODO remove this -> it's just for fixing wrong created sequences
            if (this.Id == Guid.Empty)
            {
                this.Id = Guid.NewGuid();
            }

            var persistOnlyId = GetType().GetProperties().Where(p => p.GetCustomAttributes<MTFPersistIdOnlyAttribute>(true).Any());
            if (IsLazyLoad)
            {
                //serialize just as identity object and keep object in lazy load cache
                var identity = new MTFIdentityObject
                {
                    Id = this.Id,
                    IdentifierName = "Id",
                    TypeName = this.GetType().FullName,
                    IsLazyLoad = true,
                };

                Dictionary<string, object> propertiesToSave = new Dictionary<string, object>();
                foreach (string propertyName in identity.properties.Keys)
                {
                    if (identity.properties[propertyName] == null || identity.properties[propertyName].GetType().IsSerializable)
                    {
                        propertiesToSave[propertyName] = identity.properties[propertyName];
                    }
                    else
                    {
                        propertiesToSave[propertyName] = identity.properties[propertyName].ToString();
                    }
                }

                formatter.Serialize(ms, propertiesToSave);

                this.IsLazyLoad = false;
                //if object is'nt in cache and isn't identityObject
                if (!LazyLoadCache.ContainsKey(Id) && !this.properties.ContainsKey("IdentifierName") && !this.properties.ContainsKey("TypeName"))
                {
                    LazyLoadCache[Id] = this;
                }
            }
            else if (persistOnlyId.Any())
            {
                Dictionary<string, object> propertiesToSave = new Dictionary<string, object>();
                IEnumerable<string> persitOnlyIdProperties = persistOnlyId.Select(item => item.Name);
                foreach (string propertyName in properties.Keys)
                {
                    if (properties[propertyName] != null && persitOnlyIdProperties.Contains(propertyName) && properties[propertyName].GetType() != typeof(MTFIdentityObject))
                    {
                        string identifierName = persistOnlyId.First(i => i.Name == propertyName).GetCustomAttribute<MTFPersistIdOnlyAttribute>(true).IdentifierName;
                        propertiesToSave[propertyName] = new MTFIdentityObject
                        {
                            Id = (Guid)properties[propertyName].GetType().GetProperty(identifierName).GetValue(properties[propertyName]),
                            IdentifierName = identifierName,
                            TypeName = properties[propertyName].GetType().FullName,
                        };
                    }
                    else
                    {
                        propertiesToSave[propertyName] = properties[propertyName];
                    }
                }
                formatter.Serialize(ms, propertiesToSave);
            }
            else
            {
                Dictionary<string, object> propertiesToSave = new Dictionary<string, object>();
                foreach (string propertyName in properties.Keys)
                {
                    if (properties[propertyName] == null || properties[propertyName].GetType().IsSerializable)
                    {
                        propertiesToSave[propertyName] = properties[propertyName];
                    }
                    else
                    {
                        propertiesToSave[propertyName] = properties[propertyName].ToString();
                    }
                }

                formatter.Serialize(ms, propertiesToSave);
            }

            info.AddValue("properties", ms.ToArray());
            ms.Close();
            ms.Dispose();
        }

        public MTFDataTransferObject ReplaceIdentityObjects()
        {
            var searchPath = new List<Guid>();
            var cache = buildCache(this);

            replaceIdentityObject(this, ref searchPath, ref cache);

            return this;
        }

        public MTFDataTransferObject ReplaceIdentityObjectsNoCache(MTFDataTransferObject source)
        {
            var searchPath = new List<Guid>();
            Dictionary<Guid, MTFDataTransferObject> emptyCache = null;

            replaceIdentityObject(new[] { source, this }, ref searchPath, ref emptyCache);

            return this;
        }

        public event IdentityObjectReplacedEventHandler IdentityObjectReplaced;
        public delegate void IdentityObjectReplacedEventHandler(MTFDataTransferObject sender, MTFDataTransferObject newObject, string propertyName);

        private void replaceIdentityObject(MTFDataTransferObject rootObj, ref List<Guid> searchPath, ref Dictionary<Guid, MTFDataTransferObject> cache) => replaceIdentityObject(new[] { rootObj }, ref searchPath, ref cache);
        
        private void replaceIdentityObject(IEnumerable<MTFDataTransferObject> rootObjs, ref List<Guid> searchPath, ref Dictionary<Guid, MTFDataTransferObject> cache)
        {
            Dictionary<string, object> propertiesToReplace = new Dictionary<string, object>();
            foreach (string propertyName in properties.Keys)
            {
                if (properties[propertyName] is MTFIdentityObject)
                {
                    if (cache != null)
                    {

                        propertiesToReplace[propertyName] = findSubObject(properties[propertyName] as MTFIdentityObject, ref cache);
                    }
                    else
                    {
                        List<Guid> searchedObjects = new List<Guid>();

                        foreach (var rootObj in rootObjs)
                        {
                            MTFDataTransferObject res = null;
                            if (rootObj.findSubObject(((MTFDataTransferObject)properties[propertyName]).Id, ref searchedObjects, out res) && !(res is MTFIdentityObject))
                            {
                                propertiesToReplace[propertyName] = res;
                                break;
                            }
                        }
                    }
                }
                else if (properties[propertyName] is ICollection)
                {
                    foreach (object item in ((ICollection)properties[propertyName]))
                    {
                        if (item is MTFDataTransferObject)
                        {
                            var dataTransferObj = item as MTFDataTransferObject;
                            //detect cycle
                            if (!searchPath.Contains(this.Id))
                            {
                                searchPath.Add(this.Id);
                                dataTransferObj.replaceIdentityObject(rootObjs, ref searchPath, ref cache);
                                searchPath.Remove(this.Id);
                            }
                        }
                    }
                }
                else if (properties[propertyName] is MTFDataTransferObject)
                {
                    //detect cycle
                    if (!searchPath.Contains(this.Id))
                    {
                        searchPath.Add(this.Id);
                        ((MTFDataTransferObject)properties[propertyName]).replaceIdentityObject(rootObjs, ref searchPath, ref cache);
                        searchPath.Remove(this.Id);
                    }
                }
            }

            foreach (string propertyName in propertiesToReplace.Keys)
            {
                properties[propertyName] = propertiesToReplace[propertyName];
                IdentityObjectReplaced?.Invoke(this, (MTFDataTransferObject)properties[propertyName], propertyName);
            }
        }

        public int GetObjectCount<T>()
        {
            int c = 0;
            this.foreachProperty((propertyName, mtfDto) =>
            {
                if (propertyName == nameof(Id) && mtfDto is T)
                {
                    c++;
                }
            });

            return c;
        }

        private static Dictionary<Guid, MTFDataTransferObject> buildCache(MTFDataTransferObject rootObj)
        {
            Dictionary<Guid, MTFDataTransferObject> cache = new Dictionary<Guid, MTFDataTransferObject>();
            rootObj.foreachProperty((propertyName, mtfDto) =>
            {
                if (propertyName == nameof(Id) && !(mtfDto is MTFIdentityObject))
                {
                    cache[mtfDto.Id] = mtfDto;
                }
            });

            return cache;
        }

        private static MTFDataTransferObject findSubObject(MTFIdentityObject identity, ref Dictionary<Guid, MTFDataTransferObject> cache)
        {
            if (cache.ContainsKey(identity.Id))
            {
                return cache[identity.Id];
            }

            return null;
        }

        private bool findSubObject(Guid id, ref List<Guid> searchedObjects, out MTFDataTransferObject result)
        {
            if (this.Id == id)
            {
                result = this;
                return true;
            }

            //search in first level
            foreach (var propertyName in properties.Keys)
            {
                if (properties[propertyName] is MTFDataTransferObject && ((MTFDataTransferObject)properties[propertyName]).Id == id)
                {
                    result = (MTFDataTransferObject)properties[propertyName];
                    return true;
                }
                else if (properties[propertyName] is ICollection)
                {
                    foreach (var item in (ICollection)properties[propertyName])
                    {
                        if (item is MTFDataTransferObject && ((MTFDataTransferObject)item).Id == id)
                        {
                            result = (MTFDataTransferObject)item;
                            return true;
                        }
                    }
                }
            }

            //search in all mtf sub objects - DEEP SEARCH - must detect cycle
            foreach (var propertyName in properties.Keys)
            {
                if (properties[propertyName] is MTFServiceCommand)
                {
                    // hofix because algorithm unnecessarly browses through the activities joined to the command
                    continue;
                }
                if (properties[propertyName] is MTFDataTransferObject)
                {
                    if (!searchedObjects.Contains(this.Id))
                    {
                        searchedObjects.Add(this.Id);
                        MTFDataTransferObject dto = null;
                        var ok = ((MTFDataTransferObject)properties[propertyName]).findSubObject(id, ref searchedObjects, out dto);
                        searchedObjects.Remove(this.Id);

                        if (ok)
                        {
                            result = dto;
                            return true;
                        }
                    }
                }
                else if (properties[propertyName] is ICollection)
                {
                    foreach (var item in (ICollection)properties[propertyName])
                    {
                        if (item is MTFDataTransferObject)
                        {
                            if (!searchedObjects.Contains(this.Id))
                            {
                                searchedObjects.Add(this.Id);
                                MTFDataTransferObject dto = null;
                                var ok = ((MTFDataTransferObject)item).findSubObject(id, ref searchedObjects, out dto);
                                searchedObjects.Remove(this.Id);

                                if (ok)
                                {
                                    result = dto;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            result = null;
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (RaiseNotifyPropertyChangedEvent && !string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        bool isCallingChildmemberChanged = false;
        protected virtual void ChildMemberChanged(string memberName, string memberPropertyName)
        {
            //Detect cycle and stop calling notify property changed in endless loop
            if (RaiseNotifyPropertyChangedEvent && !isCallingChildmemberChanged)
            {
                isCallingChildmemberChanged = true;
                NotifyPropertyChanged(string.Format("{0}.{1}", memberName, memberPropertyName));
                isCallingChildmemberChanged = false;
            }
        }
        
        protected void AdoptMember(object newMember, string memberName)
        {
            if (newMember is ICollection)
            {
                AdoptCollectionMember((ICollection)newMember, memberName);
            }
            else
            {
                AdoptSingleMember(newMember, memberName);
            }
        }

        private void AdoptSingleMember(object newMember, string memberName)
        {
            if (newMember is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)newMember).PropertyChanged += (s, e) => { ChildMemberChanged(memberName, e.PropertyName); };
            }

            MTFDataTransferObject mtfObject = newMember as MTFDataTransferObject;
            if (mtfObject != null)
            {
                mtfObject.Parent = this;
            }
        }

        private bool isPersistIdOnlyProperty(string propertyName)
        {
            var property = GetType().GetProperties().FirstOrDefault(p => p.Name == propertyName && p.GetCustomAttribute<MTFPersistIdOnlyAttribute>() != null);

            return property != null;
        }

        private void AdoptCollectionMember(ICollection collection, string memberName)
        {
            if (collection is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)collection).CollectionChanged += (s, e) =>
                    {
                        //NotifyPropertyChanged(memberName);
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            foreach (object item in e.NewItems)
                            {
                                AdoptMember(item, memberName);
                            }
                        }
                        this.IsModified = true;
                    };
            }
            foreach (var item in collection)
            {
                AdoptMember(item, memberName);
            }
        }

        protected virtual object CloneInternal(bool copyIdValue)
        {
            MTFDataTransferObject clone = Activator.CreateInstance(GetType()) as MTFDataTransferObject;
            var onlyIdProperties = GetType().GetProperties().Where(p => p.GetCustomAttribute<MTFPersistIdOnlyAttribute>() != null);

            foreach (string propertyName in properties.Keys)
            {
                if (onlyIdProperties.Any(pi => pi.Name == propertyName))
                {
                    var propertyInfo = onlyIdProperties.First(pi => pi.Name == propertyName);
                    clone.properties[propertyName] = cloneAsIdentityObject(propertyName,
                        properties[propertyName],
                        propertyInfo.PropertyType.FullName,
                        propertyInfo.GetCustomAttribute<MTFPersistIdOnlyAttribute>());
                }
                else
                {
                    clone.CloneProperty(propertyName, properties[propertyName]);
                    if (RaiseNotifyPropertyChangedEvent)
                    {
                        clone.AdoptMember(clone.properties[propertyName], propertyName);
                    }
                }
            }

            return clone;
        }
        public virtual object ShallowCopy()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Createy dummy copy of object - ID is same as in original object
        /// </summary>
        /// <returns></returns>
        public object Copy()
        {
            return Clone();
        }

        /// <summary>
        /// Create clone of object with new ID
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return CloneInternal(false);
        }

        public void ForEach<T>(Action<T> func)
        {
            forEachProcessMember(new List<Guid>(), func, this);
        }

        private void forEachProcessMember<T>(List<Guid> searchPath, Action<T> func, object member)
        {
            if (member is MTFIdentityObject)
            {
                return;
            }

            if (member is T)
            {
                func((T)member);
            }

            if (member is ICollection)
            {
                forEachProcessCollection(searchPath, func, (ICollection)member);
            }
            else if (member is MTFDataTransferObject)
            {
                MTFDataTransferObject dto = member as MTFDataTransferObject;
                //detect cycle
                if (!searchPath.Contains(dto.Id))
                {
                    searchPath.Add(dto.Id);
                    foreach (var propertyName in dto.properties.Keys.ToArray())
                    {
                        forEachProcessMember(searchPath, func, dto.properties[propertyName]);
                    }
                    searchPath.Remove(dto.Id);
                }
            }
        }

        private void forEachProcessCollection<T>(List<Guid> searchPath, Action<T> func, ICollection collection)
        {
            foreach (var member in collection)
            {
                forEachProcessMember(searchPath, func, member);
            }
        }

        private object cloneAsIdentityObject(string propertyName, object propertyValue, string propertyTypeName, MTFPersistIdOnlyAttribute persistIdOnly)
        {
            var origin = properties[propertyName] as MTFDataTransferObject;
            if (origin == null)
            {
                return null;
            }
            MTFIdentityObject identityObj = new MTFIdentityObject();
            identityObj.IdentifierName = persistIdOnly.IdentifierName;
            identityObj.TypeName = propertyTypeName;
            identityObj.Id = origin.Id;

            return identityObj;
        }

        protected void CloneProperty(string propertyName, object propertyValue)
        {
            if (propertyName == nameof(Id) || propertyName == nameof(ObjectVersion))
            {
                return;
            }

            properties[propertyName] = cloneValue(propertyValue);
        }

        private object cloneValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IList)
            {
                return cloneIEnumerableValue((IList)value);
            }

            if (value is ICloneable)
            {
                return ((ICloneable)value).Clone();
            }

            return value;
        }

        private object cloneIEnumerableValue(IList value)
        {
            var type = value.GetType();
            if (type.IsArray)
            {
                Array outObj = Array.CreateInstance(type.GetElementType(), value.Count);
                for (int i = 0; i < value.Count; i++)
                {
                    outObj.SetValue(cloneValue(value[i]), i);
                }
                return outObj;
            }
            else if (type.IsGenericType)
            {
                IList outObj = Activator.CreateInstance(type) as IList;
                foreach (object item in value)
                {
                    outObj.Add(cloneValue(item));
                }
                return outObj;
            }
            return null;
        }

    }
}
