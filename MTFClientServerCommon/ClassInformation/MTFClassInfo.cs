using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFClassInfo : GenericClassInfo
    {
        public MTFClassInfo()
            : base()
        {

        }

        public MTFClassInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFClassInfo(Type monsterType)
            : base(monsterType)
        {

            //set Categories property
            var categories = monsterType.GetCustomAttributes<MTFClassCategoryAttribute>(true);
            if (categories == null || !categories.Any())
            {
                Categories = new string[] { "Other" };
            }
            else
            {
                Categories = new string[categories.Count()];
                for (int i = 0; i < categories.Count(); i++)
                {
                    Categories[i] = categories.ElementAt(i).Name;
                }
            }

            Methods = new MTFObservableCollection<MTFMethodInfo>();
            monsterType.GetMethods().
                Where(method => method.GetCustomAttributes<MTFMethodAttribute>(true).Any()).ToList().
                ForEach(monsterMethod => Methods.Add(new MTFMethodInfo(monsterMethod)));                

            Properties = new MTFObservableCollection<MTFPropertyInfo>();
            monsterType.GetProperties().
                Where(property => property.GetCustomAttributes<MTFPropertyAttribute>(true).Any()).ToList().
                ForEach(monsterProperty => Properties.Add(new MTFPropertyInfo(monsterProperty)));
        }

        protected override void SetName(Type classType)
        {
            var monsterClassAttribute = classType.GetCustomAttribute<MTFClassAttribute>(true);
            //set Name property
            Name = string.IsNullOrEmpty(monsterClassAttribute.Name) ? classType.Name : monsterClassAttribute.Name;
            //set Description property
            Description = monsterClassAttribute.Description;
            //set Icon property
            Icon = monsterClassAttribute.Icon;

            ThreadSafeLevel = monsterClassAttribute.ThreadSafeLevel;
        }

        protected override void FillConstrutors(Type classType)
        {
            Constructors = new MTFObservableCollection<MTFConstructorInfo>();
            classType.GetConstructors().
                Where(constructor => constructor.GetCustomAttributes<MTFConstructorAttribute>(true).Any()).ToList().
                ForEach(monsterConstructor => Constructors.Add(new MTFConstructorInfo(monsterConstructor)));
        }

        protected override void FillMethods(Type classType)
        {
            //do nothing, because all is done in constructor and can not be moved here.
        }

        protected override void FillProperites(Type classType)
        {
            //do nothing, because all is done in constructor and can not be moved here.
        }

        public string Description
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string[] Categories
        {
            get => GetProperty<string[]>();
            set => SetProperty(value);
        }

        public new IList<MTFConstructorInfo> Constructors
        {
            get => GetProperty<IList<MTFConstructorInfo>>();
            set => SetProperty(value);
        }

        public new IList<MTFMethodInfo> Methods
        {
            get => GetProperty<IList<MTFMethodInfo>>();
            set => SetProperty(value);
        }

        public new IList<MTFPropertyInfo> Properties
        {
            get => GetProperty<IList<MTFPropertyInfo>>();
            set => SetProperty(value);
        }

        public MTFIcons Icon
        {
            get => GetProperty<MTFIcons>();
            set => SetProperty(value);
        }

        public ThreadSafeLevel ThreadSafeLevel
        {
            get => GetProperty<ThreadSafeLevel>();
            set => SetProperty(value);
        }

        public IList<ClientContolInfo> ClientControlInfos
        {
            get => GetProperty<IList<ClientContolInfo>>();
            set => SetProperty(value);
        }

        public List<object> AllExecutables
        {
            get
            {
                var output = Methods.OrderBy(m => m.DisplayName).ToList<object>();
                List<MTFPropertyInfo> properties = new List<MTFPropertyInfo>();
                foreach (var property in Properties.OrderBy(p => p.DisplayName))
                {
                    if (property.CanRead)
                    {
                        properties.Add(new MTFPropertyInfo
                        {
                            CanRead = property.CanRead,
                            CanWrite = property.CanWrite,
                            Name = property.Name + ".Get",
                            Type = property.Type,
                            Description = property.Description,
                            DisplayName = "Get " + property.DisplayName,
                            AllowedValues = property.AllowedValues,
                            ValueName = property.ValueName,
                            ValueListName = property.ValueListName,
                            ValueListLevel = property.ValueListLevel,
                            ValueListParentName = property.ValueListParentName,
                        });
                    }
                    if (property.CanWrite)
                    {
                        properties.Add(new MTFPropertyInfo
                        {
                            CanRead = property.CanRead,
                            CanWrite = property.CanWrite,
                            Name = property.Name + ".Set",
                            Type = property.Type,
                            Description = property.Description,
                            DisplayName = "Set " + property.DisplayName,
                            AllowedValues = property.AllowedValues != null ? (MTFObservableCollection<MTFAllowedValue>)property.AllowedValues.Copy() : null,
                            ValueName = property.ValueName,
                            DefautlValue = property.DefautlValue,
                            ValueListName = property.ValueListName,
                            ValueListLevel = property.ValueListLevel,
                            ValueListParentName = property.ValueListParentName,
                        });
                    }
                }
                output.AddRange(properties);

                return output;
            }
        }
    }
}
