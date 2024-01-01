using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Mathematics;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSequenceMessageActivity : MTFSequenceActivity
    {
        private readonly List<SequenceMessageType> noBlockingMessageBoxTypes = new List<SequenceMessageType>
        { 
            SequenceMessageType.Info,
            SequenceMessageType.Warning,
            SequenceMessageType.Error
        };
        public MTFSequenceMessageActivity()
        {
        }

        public MTFSequenceMessageActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ObjectVersion
        {
            get
            {
                return "1.5.0";
            }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                Term t = properties["Header"] as Term;
                if (t != null)
                {
                    MTFStringFormat sf = new MTFStringFormat();
                    sf.Text = "{0}";
                    sf.Parameters = new MTFObservableCollection<Term>();
                    sf.Parameters.Add(t);

                    Header = sf;
                }

                t = properties["Message"] as Term;
                if (t != null)
                {
                    MTFStringFormat sf = new MTFStringFormat();
                    sf.Text = "{0}";
                    sf.Parameters = new MTFObservableCollection<Term>();
                    sf.Parameters.Add(t);

                    Message = sf;
                }
                fromVersion = "1.1.0";
            }
            if (fromVersion == "1.1.0")
            {
                if (Term == null)
                {
                    Term = new EmptyTerm();
                }
                fromVersion = "1.2.0";
            }
            if (fromVersion == "1.2.0")
            {
                if (Image == null)
                {
                    Image = new EmptyTerm();
                }
                fromVersion = "1.3.0";
            }
            if (fromVersion == "1.3.0")
            {
                VersionConvertHelper.ConvertMessageButtons(this, properties);
                fromVersion = "1.4.0";
            }
            if (fromVersion == "1.4.0")
            {
                VersionConvertHelper.ConvertMessageValues(this, properties);
                fromVersion = "1.5.0";
            }
        }

        public MTFStringFormat Header
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public MTFStringFormat Message
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        

        public MessageActivityButtons Buttons
        {
            get { return GetProperty<MessageActivityButtons>(); }
            set
            {
                if (value != Buttons)
                {
                    SetProperty(value);
                }
            }
        }

        public bool ShowCancel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public object Values
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }


        public SequenceMessageDisplayType DisplayType
        {
            get { return GetProperty<SequenceMessageDisplayType>(); }
            set { SetProperty(value); }
        }

        public SequenceMessageType MessageType
        {
            get { return GetProperty<SequenceMessageType>(); }
            set
            {
                if (value == SequenceMessageType.Choice || value == SequenceMessageType.Input)
                {
                    if (ReturnType != typeof(string).FullName)
                    {
                        ReturnType = typeof(string).FullName;
                        Term = new EmptyTerm();
                    }
                }
                else if (value == SequenceMessageType.NoBlockingMessage)
                {
                    ReturnType = typeof(void).FullName;
                    Buttons = MessageActivityButtons.Ok;
                    Term = new EmptyTerm();
                }
                else
                {
                    if (ReturnType != typeof(bool).FullName)
                    {
                        ReturnType = typeof(bool).FullName;
                        Term = new EmptyTerm();
                    }
                }
                SetProperty(value);
                ActivityName = value.Description();
            }
        }

        public List<SequenceMessageType> NoBlockingMessageBoxTypes
        {
            get { return noBlockingMessageBoxTypes; }
        }

        public SequenceMessageType NoBlockingMessageBoxType
        {
            get { return GetProperty<SequenceMessageType>(); }
            set { SetProperty(value); }
        }

        public Term Image
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }

        public bool IsFullScreen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsUseAbsolutePath
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string PathToImage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public override MTFIcons Icon
        {
            get
            {
                return MTFIcons.Message;
            }
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ActivitySource
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set { SetProperty(value); }
        }
    }

    public enum SequenceMessageType
    {
        [Description("Activity_Msg_Type_Info")]
        Info,
        [Description("Activity_Msg_Type_Warning")]
        Warning,
        [Description("Activity_Msg_Type_Error")]
        Error,
        [Description("Activity_Msg_Type_Question")]
        Question,
        [Description("Activity_Msg_Type_ImportantQuestion")]
        ImportantQuestion,
        [Description("Activity_Msg_Type_Choice")]
        Choice,
        [Description("Activity_Msg_Type_NoBlocking")]
        NoBlockingMessage,
        [Description("Activity_Msg_Type_Input")]
        Input,
        [Description("Activity_Msg_Type_Picture")]
        Picture,
        [Description("Activity_Msg_Type_Close")]
        Close,

    }

    public enum SequenceMessageDisplayType
    {
        [Description("Activity_Msg_DisplayType_ComboBox")]
        ComboBox,
        [Description("Activity_Msg_DisplayType_Buttons")]
        Buttons,
        [Description("Activity_Msg_DisplayType_ToggleButtons")]
        ToggleButtons,
    }

    public enum MessageActivityButtons
    {
        None,
        Ok,
        OkCancel,
        YesNo,
    }

}
