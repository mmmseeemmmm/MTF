using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFExecuteActivity : MTFCallActivityBase
    {
        public MTFExecuteActivity()
            : base()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public MTFExecuteActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PropertyChanged += OnPropertyChanged;
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ActivityToCall
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("RefreshName");
            }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.Execute; }
        }

        public ExecuteActyvityTypes Type
        {
            get { return GetProperty<ExecuteActyvityTypes>(); }
            set
            {
                SetProperty(value);
                if (value == ExecuteActyvityTypes.External && ExternalCall == null)
                {
                    ExternalCall = new ExternalCallInfo();
                }
                NotifyPropertyChanged("RefreshName");
            }
        }

        public DynamicActivityTypes DynamicActivityType
        {
            get { return GetProperty<DynamicActivityTypes>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("RefreshName");
            }
        }

        public MTFStringFormat DynamicSequence
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public MTFStringFormat DynamicMethod
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        //because of refreshing translated name
        public bool RefreshName
        {
            get { return false; }
        }

        public ExternalCallInfo ExternalCall
        {
            get { return GetProperty<ExternalCallInfo>(); }
            set { SetProperty(value); }
        }


        public void InvalidateExternalCall()
        {
            if (ExternalCall!=null)
            {
                ExternalCall.InvalidateExternalSubSequenceToCall();
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "ExternalCall.OriginalCallActivityName" ||
                propertyChangedEventArgs.PropertyName == "ExternalCall.CallActivityIndexer")
            {
                NotifyPropertyChanged("RefreshName");
            }
        }

    }

    public enum ExecuteActyvityTypes
    {
        [Description("Activity_CallActivity_Type_Local")]
        Local,

        [Description("Activity_CallActivity_Type_External")]
        External,

        [Description("Activity_CallActivity_Type_Dynamic")]
        Dynamic,
    }

    public enum DynamicActivityTypes
    {
        [Description("Activity_CallActivity_Dynamic_Load")]
        Load,

        [Description("Activity_CallActivity_Dynamic_Unload")]
        Unload,

        [Description("Activity_CallActivity_Dynamic_Execute")]
        Execute,
    }
}
