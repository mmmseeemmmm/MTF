using AutomotiveLighting.MTFCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericClassInstanceConfiguration : MTFDataTransferObject
    {
        private object value;

        public GenericClassInstanceConfiguration()
        {
        }

        public GenericClassInstanceConfiguration(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericClassInstanceConfiguration(GenericClassInfo classInfo)
        {
            prepireGenericParameterValues(classInfo);
        }

        public GenericClassInfo ClassInfo
        {
            get
            {
                if (value != null)
                {
                    generateData();
                }
                return GetProperty<GenericClassInfo>();
            }
            set { SetProperty(value); }
        }

        public IList<GenericParameterValue> PropertyValues
        {
            get
            {
                if (value != null)
                {
                    generateData();
                }
                return GetProperty<IList<GenericParameterValue>>();
            }
            set { SetProperty(value); }
        }

        public GenericClassInstanceConfiguration(object value)
        {
            this.value = value;
            //prepireGenericParameterValues(new GenericClassInfo(value.GetType()));
            //fillPropertyValues(value);
        }

        private void prepireGenericParameterValues(GenericClassInfo classInfo)
        {
            ClassInfo = classInfo;
            var propertyValues = new MTFObservableCollection<GenericParameterValue>();
            foreach (GenericPropertyInfo propertyInfo in classInfo.Properties)
            {
                propertyValues.Add(new GenericParameterValue(propertyInfo));
            }
            PropertyValues = propertyValues;
        }

        private void fillPropertyValues(object value)
        {
            //fill property values
            Type valueType = value.GetType();
            foreach (GenericParameterValue propertyValue in GetProperty<IList<GenericParameterValue>>("PropertyValues"))
            {
                object p = valueType.GetProperty(propertyValue.Name).GetGetMethod().Invoke(value, null);
                var typeInfo = new Helpers.TypeInfo(propertyValue.TypeName);
                if (p == null)
                {
                    //propertyValue.Value = "<null>";
                }
                else if (typeInfo.IsCollection)
                {
                    propertyValue.Value = createPropertyWithCollection((IEnumerable)p);
                }
                else if (p.GetType().GetCustomAttributes(typeof(MTFKnownClassAttribute), true).Any())
                {
                    propertyValue.Value = new GenericClassInstanceConfiguration(p);
                }
                else if (typeInfo.IsImage)
                {
                    propertyValue.Value = p;
                }
                else
                {
                    if (typeInfo.IsSimpleType)
                    {
                        propertyValue.Value = p;
                    }
                    else
                    {
                        try
                        {
                            propertyValue.Value = p.ToString();
                        }
                        catch (Exception e)
                        {
                            propertyValue.Value = e.Message;
                        }
                    }
                }
            }
        }

        private object[] createPropertyWithCollection(IEnumerable properties)
        {
            List<object> list = new List<object>();
            foreach (object listValue in properties)
            {
                if (listValue == null)
                {
                    list.Add(null);
                }
                else
                {
                    Helpers.TypeInfo listValueTypeInfo = new Helpers.TypeInfo(listValue.GetType().FullName);
                    if (listValueTypeInfo.IsCollection && listValue is IEnumerable)
                    {
                        list.Add(createPropertyWithCollection((IEnumerable)listValue));
                    }
                    else if (listValue.GetType().GetCustomAttributes(typeof(MTFKnownClassAttribute), true).Any())
                    {
                        list.Add(new GenericClassInstanceConfiguration(listValue));
                    }
                    else if (listValueTypeInfo.IsImage)
                    {
                        list.Add(listValue);
                    }
                    else
                    {
                        if (listValueTypeInfo.IsSimpleType)
                        {
                            list.Add(listValue);
                        }
                        else
                        {
                            list.Add(listValue.ToString());
                        }
                    }
                }
            }

            return list.ToArray();
        }

        private void generateData()
        {
            prepireGenericParameterValues(new GenericClassInfo(value.GetType()));
            fillPropertyValues(value);
            value = null;
        }

        protected override void BeforeGetObjectData()
        {
            base.BeforeGetObjectData();
            if (!IsLazyLoad && value != null)
            {
                generateData();
            }
        }

        public override string ToString()
        {
            if (IsLazyLoad || ClassInfo == null)
            {
                return string.Empty;
            }

            return ClassInfo.FullName;
        }
    }
}
