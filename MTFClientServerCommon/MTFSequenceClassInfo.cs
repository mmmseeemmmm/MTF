using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSequenceClassInfo : MTFDataTransferObject
    {
        public MTFSequenceClassInfo()
        {
            IsEnabled = true;
        }

        public MTFSequenceClassInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ObjectVersion
        {
            get { return "1.0.2"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);

            if (fromVersion == "1.0.0")
            {
                if (properties != null && properties.ContainsKey("Data"))
                {
                    var oldData = (Dictionary<string, byte[]>) properties["Data"];
                    var newData = new Dictionary<string, IList<ClassInfoData>>();
                    foreach (var dataName in oldData.Keys)
                    {
                        newData[dataName] = new List<ClassInfoData>
                        {
                            new ClassInfoData{SequenceVariant = null, Data = oldData[dataName], LastModifiedTime = DateTime.Now}
                        };
                    }

                    properties["Data"] = newData;
                }
                fromVersion = "1.0.1";
            }
            if (fromVersion == "1.0.1")
            {
                IsEnabled = true;
                fromVersion = "1.0.2";
            }

        }

        public string Alias
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public MTFClassInfo MTFClass
        {
            get { return GetProperty<MTFClassInfo>(); }
            set { SetProperty(value); }
        }

        public MTFClassInstanceConfiguration MTFClassInstanceConfiguration
        {
            get { return GetProperty<MTFClassInstanceConfiguration>(); }
            set { SetProperty(value); }
        }

        public void SetMTFClassInstanceConfigurationWithoutModification(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            ExecuteWithoutSetIsModified(() =>
            {
                MTFClassInstanceConfiguration = classInstanceConfiguration;
            });
        }

        public MTFClassConstructionType ConstructionType
        {
            get { return GetProperty<MTFClassConstructionType>(); }
            set { SetProperty(value); }
        }

        public Dictionary<string, IList<ClassInfoData>> Data
        {
            get { return GetProperty<Dictionary<string, IList<ClassInfoData>>>(); }
            set { SetProperty(value); }
        }

        private bool isMapped;
        public bool IsMapped
        {
            get { return isMapped; }
            set { isMapped = value; }
        }

        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        private ObservableCollection<MTFSequenceClassInfo> subComponents;

        public ObservableCollection<MTFSequenceClassInfo> SubComponents
        {
            get { return subComponents; }
            set
            {
                subComponents = value;
                NotifyPropertyChanged();
            }
        }
    }

    [Serializable]
    public class ClassInfoData
    {
        private SequenceVariant sequenceVariant;
        private byte[] data;
        private DateTime lastModifiedTime;

        public SequenceVariant SequenceVariant
        {
            get { return sequenceVariant; }
            set { sequenceVariant = value; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public DateTime LastModifiedTime
        {
            get { return lastModifiedTime; }
            set { lastModifiedTime = value; }
        }
    }

    public enum MTFClassConstructionType
    {
        [Description("On sequence start")]
        OnSequenceStart,
        [Description("Before first usage")]
        BeforeFirstUsage
    }
}
