using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSubSequenceActivity : MTFCallActivityBase
    {
        public MTFSubSequenceActivity()
            : base()
        {

        }

        public MTFSubSequenceActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.6.0" || fromVersion == "0.5.0" || fromVersion == "1.4.0" || fromVersion == "1.3.0"
                || fromVersion == "1.2.0" || fromVersion == "1.1.0" || fromVersion == "1.0.0" || fromVersion == "0.0.0")
            {
                this.OnError = MTFErrorBehavior.Continue;
                this.OnCheckOutputFailed = MTFErrorBehavior.Continue;
            }
        }

        public IList<MTFSequenceActivity> Activities
        {
            get => GetProperty<IList<MTFSequenceActivity>>();
            set => SetProperty(value);
        }

        public ExecutionType ExecutionType
        {
            get => GetProperty<ExecutionType>();
            set => SetProperty(value);
        }

        public IList<MTFCase> Cases
        {
            get => GetProperty<IList<MTFCase>>();
            set => SetProperty(value);
        }

        public string CasesType => typeof(IList<MTFCase>).FullName;

        public bool VariantSwitch
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public int ActualCaseIndex
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public bool IsExecuteAsOneActivity
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool CanExecuteService
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public ServiceExecutionBehavior ServiceExecutionBehavior
        {
            get => GetProperty<ServiceExecutionBehavior>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity PrepaireServiceActivity
        {
            get => GetProperty<MTFSequenceActivity>();
            set => SetProperty(value);
        }
        
        [MTFPersistIdOnly]
        public MTFSequenceActivity CleanupServiceActivity
        {
            get => GetProperty<MTFSequenceActivity>();
            set => SetProperty(value);
        }

        protected override void BeforeGetObjectData()
        {
            base.BeforeGetObjectData();
            MTFSequenceHelper.CleanNotUsedActivities(this);
        }

        public bool SwitchDUT
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public DeviceUnderTestInfo SwitchToDeviceUnderTestInfo
        {
            get => GetProperty<DeviceUnderTestInfo>();
            set => SetProperty(value);
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity ActivityOnCatch
        {
            get => GetProperty<MTFSequenceActivity>();
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("RefreshName");
            }
        }
    }

    public enum ServiceExecutionBehavior
    {
        [Description("Execute inner activities")]
        ExecuteInner,

        [Description("Skip inner activities")]
        SkipInner,
    }

    public enum ExecutionType
    {
        [Description("Execute always")]
        ExecuteAlways,

        [Description("Execute if")]
        ExecuteIf,

        [Description("Execute do while")]
        ExecuteUntil,

        [Description("Execute by call")]
        ExecuteByCall,

        [Description("Execute in parallel")]
        ExecuteInParallel,

        [Description("Execute by case")]
        ExecuteByCase,

        [Description("Execute while")]
        ExecuteWhile,

        [Description("Execute on background")]
        ExecuteOnBackground,
    }
}
