using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSequenceHandlingActivity : MTFSequenceActivity
    {
        public MTFSequenceHandlingActivity()
            : base()
        {
            StatusLinesFontSize = new StatusLinesFontSize();
            SwitchGraphicalView = new SwitchGraphicalView();
            SetUserIndicatorSettings = new SetUserIndicatorSettings();
        }

        public MTFSequenceHandlingActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ObjectVersion
        {
            get
            {
                return "1.8.0";
            }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.2.0")
            {
                List<Term> terms;
                if (properties.ContainsKey("StatusLines"))
                {
                    terms = properties["StatusLines"] as List<Term>;
                    if (terms != null)
                    {
                        StatusLines = new MTFObservableCollection<MTFStringFormat>();
                        foreach (Term t in terms)
                        {
                            StatusLines.Add(new MTFStringFormat { Text = "{0}", Parameters = new List<Term> { t } });
                        }
                    }
                }

                if (properties.ContainsKey("Logs"))
                {
                    terms = properties["Logs"] as List<Term>;
                    if (terms != null)
                    {
                        Logs = new MTFObservableCollection<MTFStringFormat>();
                        foreach (Term t in terms)
                        {
                            Logs.Add(new MTFStringFormat { Text = "{0}", Parameters = new List<Term> { t } });
                        }
                    }
                }
                fromVersion = "1.3.0";
            }
            if (fromVersion == "1.3.0")
            {
                SaveToTxt = true;
                CleanErrorMemory = true;
                RestartTimer = true;
                SetStatus = true;
                fromVersion = "1.4.0";
            }
            if (fromVersion == "1.4.0")
            {
                StatusLinesFontSize = new StatusLinesFontSize();
                fromVersion = "1.5.0";
            }
            if (fromVersion == "1.5.0")
            {
                IncludeValidationTables = true;
                fromVersion = "1.6.0";
            }
            if (fromVersion == "1.6.0")
            {
                LogCycleName = new MTFStringFormat();
                fromVersion = "1.7.0";
            }
            if (fromVersion == "1.7.0")
            {
                if (ReturnType == typeof(bool).FullName)
                {
                    var constantTerm = Term as ConstantTerm;
                    if (constantTerm != null && constantTerm.ResultType == typeof(string) && constantTerm.Value as string == string.Empty)
                    {
                        Term = new EmptyTerm();
                    } 
                }
                fromVersion = "1.8.0";
            }
        }

        public IList<MTFStringFormat> Logs
        {
            get { return GetProperty<IList<MTFStringFormat>>(); }
            set { SetProperty(value); }
        }

        public IList<MTFStringFormat> StatusLines
        {
            get { return GetProperty<IList<MTFStringFormat>>(); }
            set { SetProperty(value); }
        }

        public MTFStringFormat LogMessage
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public MTFStringFormat LogCycleName
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public bool UseCycleName
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        //public bool ClearTimeView
        //{
        //    get { return GetProperty<bool>(); }
        //    set { SetProperty(value); }
        //}

        public bool ClearAllTables
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool LogHiddenRows
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IList<TableSetting> TablesSetting
        {
            get { return GetProperty<IList<TableSetting>>(); }
            set { SetProperty(value); }
        }

        public IList<ServiceCommandsSetting> CommandsSetting
        {
            get { return GetProperty<IList<ServiceCommandsSetting>>(); }
            set { SetProperty(value); }
        }

        public SequenceHandlingType SequenceHandlingType
        {
            get { return GetProperty<SequenceHandlingType>(); }
            set
            {
                SetProperty(value);
                ActivityName = value.Description();
               
                switch (value)
                {
                    case SequenceHandlingType.GetMTFVersion:
                    case SequenceHandlingType.GetVariant: ReturnType = typeof(string).FullName; break;
                    case SequenceHandlingType.GetLoggedUsers: ReturnType = typeof(string[]).FullName; break;
                    default: ReturnType = typeof(bool).FullName; break;
                }
                //ReturnType = value == SequenceHandlingType.GetVariant ? typeof(string).FullName : typeof(bool).FullName;
            }
        }

        public bool SaveToTxt
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CleanErrorMemory
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CleanErrorWindow
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CleanTables
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool RestartTimer
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool SetStatus
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IncludeValidationTables
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        //this flag is used just for start and pause execution activity
        public bool ExecuteOnlyInDebug
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsServiceMode
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public bool IsTeachMode
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public bool GoingToServiceMode
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public bool GoingToTeachMode
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool SaveGraphicalView
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SequenceVariant SequenceVariant
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        public StatusLinesFontSize StatusLinesFontSize
        {
            get { return GetProperty<StatusLinesFontSize>(); }
            set { SetProperty(value); }
        }

        public SwitchGraphicalView SwitchGraphicalView
        {
            get { return GetProperty<SwitchGraphicalView>(); }
            set { SetProperty(value); }
        }

        public IList<UserCommandsState> UserCommandsSetting
        {
            get => GetProperty<IList<UserCommandsState>>();
            set => SetProperty(value);
        }

        public SetUserIndicatorSettings SetUserIndicatorSettings
        {
            get => GetProperty<SetUserIndicatorSettings>();
            set => SetProperty(value);
        }

        public string ExpectedCount
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get
            {
                return AutomotiveLighting.MTFCommon.MTFIcons.Sequence;
            }
        }
    }

    [Serializable]
    public class TableSetting : MTFDataTransferObject
    {
        public TableSetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public TableSetting()
            : base()
        {
        }

        [MTFPersistIdOnly]
        public MTFValidationTable.MTFValidationTable ValidationTable
        {
            get { return GetProperty<MTFValidationTable.MTFValidationTable>(); }
            set { SetProperty(value); }
        }
        public bool Clear
        {
            get { return this.GetProperty<bool>(); }
            set { SetProperty(value); }
        }

    }

    [Serializable]
    public class ServiceCommandsSetting : MTFDataTransferObject
    {
        public ServiceCommandsSetting()
            : base()
        {

        }
        public ServiceCommandsSetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [MTFPersistIdOnly]
        public MTFServiceCommand ServiceCommand
        {
            get { return this.GetProperty<MTFServiceCommand>(); }
            set { SetProperty(value); }
        }

        public bool IsEnabled
        {
            get { return this.GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool? IsChecked
        {
            get { return this.GetProperty<bool?>(); }
            set { SetProperty(value); }
        }
    }

    [Serializable]
    public class UserCommandsState : MTFDataTransferObject
    {
        public UserCommandsState()
            : base()
        {

        }
        public UserCommandsState(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [MTFPersistIdOnly]
        public MTFUserCommand UserCommand
        {
            get => GetProperty<MTFUserCommand>();
            set => SetProperty(value); 
        }

        public bool? IsEnabled
        {
            get => GetProperty<bool?>(); 
            set => SetProperty(value); 
        }

        public bool? IsChecked
        {
            get => GetProperty<bool?>();
            set => SetProperty(value); 
        }

        public bool IsExecuting
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
    }

    [Serializable]
    public class StatusLinesFontSize : MTFDataTransferObject
    {
        public StatusLinesFontSize()
            : base()
        {
            Line1 = 50;
            Line2 = 10;
            Line3 = 10;
        }

        public StatusLinesFontSize(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public int Line1
        {
            get { return this.GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Line2
        {
            get { return this.GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Line3
        {
            get { return this.GetProperty<int>(); }
            set { SetProperty(value); }
        }
    }

    [Serializable]
    public class SwitchGraphicalView : MTFDataTransferObject
    {
        public SwitchGraphicalView()
            : base()
        {
        }

        public SwitchGraphicalView(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Guid SelectedGraphicalViewId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        public SequenceExecutionViewType SwitchToView
        {
            get { return GetProperty<SequenceExecutionViewType>(); }
            set { SetProperty(value); }
        }
    }

    [Serializable]
    public class SetUserIndicatorSettings : MTFDataTransferObject
    {
        public SetUserIndicatorSettings()
            : base()
        {
        }

        public SetUserIndicatorSettings(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Guid IndicatorId
        {
            get => GetProperty<Guid>();
            set => SetProperty(value);
        }

        public Term ValueTerm
        {
            get => GetProperty<Term>();
            set => SetProperty(value);
        }
    }

    public enum SequenceHandlingType
    {
        [Description("Activity_SeqHandling_Type_SaveReport")]
        SaveReportAndCleanErrors = 10,
        [Description("Activity_SeqHandling_Type_StartCounter")]
        StartDurationCounter = 11,
        [Description("Activity_SeqHandling_Type_SetStatusMsg")]
        SetSequenceStatusMessage = 12,
        [Description("Activity_SeqHandling_Type_ClearValidTables")]
        ClearValidTables = 13,
        [Description("Activity_SeqHandling_Type_LogMsg")]
        LogMessage = 14,
        [Description("Activity_SeqHandling_Type_GetSeqVariant")]
        GetVariant = 15,
        [Description("Activity_SeqHandling_Type_SetSeqVariant")]
        SetVariant = 16,
        [Description("Activity_SeqHandling_Type_ChangeServiceCmd")]
        ChangeCommandsStatus = 17,
        [Description("Activity_SeqHandling_Type_PauseSeq")]
        PauseSequence = 18,
        [Description("Activity_SeqHandling_Type_StopSeq")]
        StopSequence = 19,
        [Description("Activity_SeqHandling_Type_GetExecState")]
        GetExecutionState = 20,
        [Description("Activity_SeqHandling_Type_GetUsers")]
        GetLoggedUsers = 21,
        [Description("Activity_SeqHandling_Type_SwitchView")]
        SwitchView = 22,
        [Description("Activity_SeqHandling_Type_ChangeUserCmd")]
        ChangeUserCommandsStatus = 23,
        [Description("Activity_SeqHandling_Type_SetUserIndicatorValue")]
        SetUserIndicatorValue = 24,
        [Description("Activity_SeqHandling_Type_SynchronizationExecuteInParallel")]
        SynchronizationExecuteInParallel = 25,
        [Description("Activity_SeqHandling_Type_GetMTFVersion")]
        GetMTFVersion = 26
    }




}
