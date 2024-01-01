using System;
using System.Runtime.Serialization;
using MTFClientServerCommon.GraphicalView;

namespace MTFClientServerCommon
{
    [Serializable]
    public class DeviceUnderTestInfo : MTFDataTransferObject
    {
        public DeviceUnderTestInfo()
        {

        }

        public DeviceUnderTestInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public GraphicalViewInfo StartupGraphicalView
        {
            get => GetProperty<GraphicalViewInfo>();
            set => SetProperty(value);
        }
    }
}
