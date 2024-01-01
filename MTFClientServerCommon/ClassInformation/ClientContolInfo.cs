using System;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.ClassInformation;

namespace MTFClientServerCommon
{
    [Serializable]
    public class ClientContolInfo : MTFDataTransferObject
    {
        public ClientContolInfo()
        {
        }

        public ClientContolInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value);}
        }

        public string Description
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string TypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string AssemblyName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public MTFIcons Icon
        {
            get { return GetProperty<MTFIcons>(); }
            set { SetProperty(value); }
        }

        public ClientControlType Type
        {
            get { return GetProperty<ClientControlType>(); }
            set { SetProperty(value); }
        }
    }
}
