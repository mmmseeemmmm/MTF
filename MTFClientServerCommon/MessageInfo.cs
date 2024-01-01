using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MessageInfo : MTFDataTransferObject
    {
        public MessageInfo()
            : base()
        {
        }

        public MessageInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Header
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Text
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool UsePicture
        {
            get { return Type == SequenceMessageType.Picture || (Type == SequenceMessageType.NoBlockingMessage && ImageData != null); }
        }

        public SequenceMessageType Type
        {
            get { return GetProperty<SequenceMessageType>(); }
            set { SetProperty(value); }
        }

        public MessageButtons Buttons
        {
            get { return GetProperty<MessageButtons>(); }
            set { SetProperty(value); }
        }

        public List<Guid> ActivityPath
        {
            get { return GetProperty<List<Guid>>(); }
            set { SetProperty(value); }
        }

        public bool IsFullScreen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SequenceMessageType AdditionalType
        {
            get { return GetProperty<SequenceMessageType>(); }
            set { SetProperty(value); }
        }

        public SequenceMessageDisplayType ChoiceDisplayType
        {
            get { return GetProperty<SequenceMessageDisplayType>(); }
            set { SetProperty(value); }
        }

        public List<string> ButtonValues
        {
            get { return GetProperty<List<string>>(); }
            set { SetProperty(value); }
        }

        public byte[] ImageData
        {
            get { return GetProperty<byte[]>(); }
            set { SetProperty(value); }
        }
    }


    public enum MessageButtons
    {
        None,
        Ok,
        Cancel,
        YesNo,
        OkCancel,
        YesNoCancel,
    }
}
