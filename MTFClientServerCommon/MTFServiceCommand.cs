using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFServiceCommand : MTFDataTransferObject
    {
        public MTFServiceCommand()
        {
            
        }

        public MTFServiceCommand(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public MTFServiceCommandIcon Icon
        {
            get { return GetProperty<MTFServiceCommandIcon>(); }
            set { SetProperty(value); }
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity PrepairActivity
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set { SetProperty(value); }
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ExecuteActivity
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set { SetProperty(value); }
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity CleaunupActivity
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set { SetProperty(value); }
        }

        public IList<MTFServiceModeVariants> UsedServiceVariants
        {
            get { return GetProperty<IList<MTFServiceModeVariants>>(); }
            set { SetProperty(value); }
        }

        public Point ServiceLocation
        {
            get { return GetProperty<Point>(); }
            set { SetProperty(value); }
        }

        public Point TeachLocation
        {
            get { return GetProperty<Point>(); }
            set { SetProperty(value); }
        }

        public MTFServiceCommandType Type
        {
            get { return GetProperty<MTFServiceCommandType>(); }
            set { SetProperty(value); }
        }
    }
}
