using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFParameterInfo : GenericParameterInfo
    {
        public MTFParameterInfo()
            : base()
        {
        }

        public MTFParameterInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFParameterInfo(ParameterInfo parameterInfo)
            : base(parameterInfo)
        {
        }

        public string Description
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ValueName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SetupMethodName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DefautlValue
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsOptional
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string MinValue
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string MaxValue
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
