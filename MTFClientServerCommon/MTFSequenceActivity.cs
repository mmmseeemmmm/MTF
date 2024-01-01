using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(MTFSubSequenceActivity))]
    [System.Xml.Serialization.XmlInclude(typeof(MTFExecuteActivity))]
    public class MTFSequenceActivity : MTFDataTransferObject, ISequenceClassInfo
    {
        public MTFSequenceActivity()
        {
        }

        public MTFSequenceActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion
        {
            get
            {
                return "1.9.0";
            }
        }

        public override string ToString()
        {
            return ActivityName;
        }

        protected override void VersionConvert(string fromVersion)
        {

            base.VersionConvert(fromVersion);

            if (fromVersion == "0.0.0")
            {
                if (properties != null && properties.ContainsKey("ActivityId"))
                {
                    properties["Id"] = properties["ActivityId"];
                    properties.Remove("ActivityId");
                }
            }
            if (fromVersion == "1.0.0" || fromVersion == "0.0.0")
            {

                if (properties != null && properties.ContainsKey("MTFParameters") && properties["MTFParameters"] != null)
                {
                    var parameters = properties["MTFParameters"] as ObservableCollection<MTFParameterValue>;
                    Helpers.VersionConvertHelper.TransformValuesToConstantTerm(parameters);
                }
            }
            if (fromVersion == "1.1.0")
            {

                if (properties != null && properties.ContainsKey("MTFParameters") && properties["MTFParameters"] != null)
                {
                    var parameters = properties["MTFParameters"] as ObservableCollection<MTFParameterValue>;
                    Helpers.VersionConvertHelper.FindAndReplaceGenericCollectionOfBaseType(parameters);
                }
            }
            if (fromVersion == "1.2.0")
            {
                if (properties != null)
                {
                    if (properties.ContainsKey("MTFParameters") && properties["MTFParameters"] != null)
                    {
                        var parameters = properties["MTFParameters"] as ObservableCollection<MTFParameterValue>;
                        properties["MTFParameters"] = Helpers.VersionConvertHelper.ReplaceBusCommunicationDriverStructures(parameters);
                    }
                    if (properties.ContainsKey("ReturnType") && properties["ReturnType"] != null)
                    {
                        if (properties["ReturnType"].ToString() == "ALBusComDriver.OffBoardConfig")
                        {
                            properties["ReturnType"] = "MTFBusCommunication.Structures.MTFOffBoardConfig";
                        }
                        else if (properties["ReturnType"].ToString() == "ALBusComDriver.OnBoardSignal")
                        {
                            properties["ReturnType"] = "MTFBusCommunication.Structures.MTFOnBoardSignal";
                        }
                        else if (properties["ReturnType"].ToString().Contains("ALBusComDriver"))
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            if (fromVersion == "1.4.0")
            {
                MTFMethodDescription = string.Empty;
            }
            if (fromVersion == "1.5.0")
            {
                if (ReturnType == "GeneralDriver.DateTime")
                {
                    ReturnType = "AutomotiveLighting.MTFCommon.Types.MTFDateTime";
                }
            }
            if (fromVersion == "1.7.0" || fromVersion == "1.6.0" || fromVersion == "1.5.0" || fromVersion == "1.4.0" || fromVersion == "1.3.0" || fromVersion == "1.2.0" || fromVersion == "1.1.0" || fromVersion == "1.0.0" || fromVersion == "0.0.0")
            {
                OnCheckOutputFailed = OnError;
                fromVersion = "1.8.0";
            }

            if (fromVersion == "1.8.0")
            {
                VersionConvertHelper.CleanOldProperties(this);
                fromVersion = "1.9.0";
            }
        }

        protected override object CloneInternal(bool copyIdValue)
        {
            MTFDataTransferObject activity = base.CloneInternal(copyIdValue) as MTFDataTransferObject;
            activity.ChangeId(Id, activity.Id);
            return activity;
        }

        public string ActivityName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int UniqueIndexer
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool IsActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string MTFClassAlias
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string MTFMethodName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string MTFMethodDescription
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool SetupModeSupport
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IList<string> UsedDataNames
        {
            get { return GetProperty<IList<string>>(); }
            set { SetProperty(value); }
        }

        public string MTFMethodDisplayName
        {
            get { return GetProperty<string>(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    ActivityName = value;
                }
                //if (MTFMethodDisplayName == null && !string.IsNullOrEmpty(value))
                //{
                //    this.ActivityName = value;
                //}
                SetProperty(value);
            }
        }

        public string ReturnType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        //public bool WriteToLog
        //{
        //    get { return GetProperty<bool>(); }
        //    set { SetProperty(value); }
        //}

        public bool RunOnce
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public MTFErrorBehavior OnCheckOutputFailed
        {
            get { return GetProperty<MTFErrorBehavior>(); }
            set { SetProperty(value); }
        }

        public bool RepeatOnCheckOutputFailed
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public uint NumberOfAttemptsOnCheckOutputFailed
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public uint RepeatDelayOnCheckOutputFailed
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public MTFErrorBehavior OnError
        {
            get { return GetProperty<MTFErrorBehavior>(); }
            set { SetProperty(value); }
        }

        public bool Repeat
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public uint NumberOfAttempts
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public uint RepeatDelay
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public IList<MTFParameterValue> MTFParameters
        {
            get { return GetProperty<IList<MTFParameterValue>>(); }
            set { SetProperty(value); }
        }

        public string Comment
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        //can be removed, but sequence cannot be loaded
        public int CountInTerm
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string NumberTypeName
        {
            get
            {
                return typeof(uint).FullName;
            }
        }

        public Term Term
        {
            get { return GetProperty<Term>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged();
            }
        }

        [MTFPersistIdOnly]
        public MTFVariable ErrorOutput
        {
            get { return GetProperty<MTFVariable>(); }
            set { SetProperty(value); }
        }

        [MTFPersistIdOnly]
        public MTFVariable Variable
        {
            get { return GetProperty<MTFVariable>(); }
            set { SetProperty(value); }
        }

        [MTFPersistIdOnly]
        public MTFSequenceClassInfo ClassInfo
        {
            get { return GetProperty<MTFSequenceClassInfo>(); }
            set { SetProperty(value); }
        }

        public bool IsPossibilityCatch
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public virtual AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get
            {
                if (ClassInfo == null || ClassInfo.MTFClass == null)
                {
                    return AutomotiveLighting.MTFCommon.MTFIcons.None;
                }
                return ClassInfo.MTFClass.Icon;
            }
        }

        //because of UI refresh
        public bool Refresh
        {
            get { return false; }
        }

        //because of UI refresh
        public void InvalidateVisual()
        {
            ExecuteWithoutSetIsModified(() => NotifyPropertyChanged("Refresh"));
        }

    }

    public enum MTFErrorBehavior
    {
        [Description("MTF_ErrorBehavior_Abort")]
        AbortTest,
        [Description("MTF_ErrorBehavior_Parent")]
        HandledByParent,
        [Description("MTF_ErrorBehavior_Continue")]
        Continue
    }
}
