using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFClassInstanceConfiguration : MTFPersist
    {
        public MTFClassInstanceConfiguration()
            : base()
        {
        }

        public MTFClassInstanceConfiguration(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFClassInstanceConfiguration(string name, MTFClassInfo classInfo, 
            MTFConstructorInfo constructorInfo)
        {
            Name = name;
            ClassInfo = classInfo;
            ParameterValues = new MTFObservableCollection<MTFParameterValue>();

            foreach (MTFParameterInfo parameter in constructorInfo.Parameters)
            {
                ParameterValues.Add(new MTFParameterValue(parameter));
            }
        }

        public MTFClassInfo ClassInfo
        {
            get { return GetProperty<MTFClassInfo>(); }
            set { SetProperty(value); }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<MTFParameterValue> ParameterValues
        {
            get { return GetProperty<IList<MTFParameterValue>>(); }
            set { SetProperty(value); }        
        }

        public IList<MTFValueList> ValueLists
        {
            get { return GetProperty<IList<MTFValueList>>(); }
            set { SetProperty(value); }
        }
    }
}
