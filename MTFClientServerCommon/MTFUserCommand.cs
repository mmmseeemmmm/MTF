using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFUserCommand : MTFDataTransferObject
    {
        public MTFUserCommand()
        {
        }

        public MTFUserCommand(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public MTFServiceCommandIcon Icon
        {
            get => GetProperty<MTFServiceCommandIcon>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ExecuteActivity
        {
            get => GetProperty<MTFSequenceActivity>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ToggleOffActivity
        {
            get => GetProperty<MTFSequenceActivity>();
            set => SetProperty(value);
        }

        public MTFUserCommandType Type
        {
            get => GetProperty<MTFUserCommandType>();
            set => SetProperty(value);
        }

        public MTFUserCommandAccessRole AccessRole
        {
            get => GetProperty<MTFUserCommandAccessRole>();
            set => SetProperty(value);
        }
    }
}
