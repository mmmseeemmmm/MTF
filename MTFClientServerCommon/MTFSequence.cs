using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSequence : MTFPersist
    {


        public MTFSequence()
        {
            VariantGroups = SequenceVariantHelper.GenerateSequenceVariants();
            ServiceDesignSetting = new MTFServiceDesignSetting();
            SequenceExecutionUiSetting = new SequenceExecutionUISetting();
            GoldSampleSetting = new GoldSampleSetting();
            GoldSampleSetting.CreateGoldSampleSelector(VariantGroups);
            SequenceVersion = new Version(0, 0, 0, 0);
            GraphicalViewSetting = new GraphicalViewSetting();
        }

        public MTFSequence(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //Extension by the new NOK Golden samples
            //if (VariantGroups[3].Values.Count == 7)
            //{
            //    VariantGroups[3].Values.Add(new SequenceVariantValue
            //    {
            //        Name = $"{GoldSampleSetting.NokGoldSample} {6}",
            //    });
            //    VariantGroups[3].Values.Add(new SequenceVariantValue
            //    {
            //        Name = $"{GoldSampleSetting.NokGoldSample} {7}",
            //    });
            //    VariantGroups[3].Values.Add(new SequenceVariantValue
            //    {
            //        Name = $"{GoldSampleSetting.NokGoldSample} {8}",
            //    });
            //    VariantGroups[3].Values.Add(new SequenceVariantValue
            //    {
            //        Name = $"{GoldSampleSetting.NokGoldSample} {9}",
            //    });
            //    VariantGroups[3].Values.Add(new SequenceVariantValue
            //    {
            //        Name = $"{GoldSampleSetting.NokGoldSample} {10}",
            //    });
            //}
        }

        public override string ObjectVersion => "1.0.26";

        //need just on client
        public MTFSequence ParentSequence { get; set; }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "0.0.0" || fromVersion == "1.0.0")
            {
                VersionConvertHelper.ReplaceSequenceHandlingActivities(this);
                fromVersion = "1.0.1";
            }
            if (fromVersion == "1.0.1")
            {
                VersionConvertHelper.FindAndReplaceNullActivities(this);
                fromVersion = "1.0.2";
            }
            if (fromVersion == "1.0.2")
            {
                VersionConvertHelper.ReplaceClassInfo(this);
                fromVersion = "1.0.3";
            }
            if (fromVersion == "1.0.3")
            {
                if (properties != null && properties.ContainsKey("MTFVariables"))
                {
                    ObservableCollection<MTFVariable> mtfVariables = properties["MTFVariables"] as ObservableCollection<MTFVariable>;
                    if (mtfVariables != null && mtfVariables.Count > 0)
                    {
                        properties["MTFVariables"] = new ObservableCollection<MTFVariable>(mtfVariables.OrderBy(x => x.Name).ToList());
                    }
                }
                fromVersion = "1.0.4";
            }
            if (fromVersion == "1.0.4")
            {
                if (properties != null && properties.ContainsKey("Data"))
                {
                    properties.Remove("Data");
                }
                fromVersion = "1.0.5";
            }
            if (fromVersion == "1.0.5")
            {
                if (properties != null && properties.ContainsKey("MTFVariables"))
                {
                    if (properties["MTFVariables"] is ObservableCollection<MTFVariable>)
                    {
                        properties["MTFVariables"] = new MTFObservableCollection<MTFVariable>((ObservableCollection<MTFVariable>)properties["MTFVariables"]);
                    }
                }
                fromVersion = "1.0.6";
            }
            if (fromVersion == "1.0.6")
            {
                VersionConvertHelper.ReplaceLoggingActivities(this);
                fromVersion = "1.0.7";
            }

            if (fromVersion == "1.0.7")
            {
                VariantGroups = SequenceVariantHelper.GenerateSequenceVariants();
                fromVersion = "1.0.8";
            }
            if (fromVersion == "1.0.8")
            {
                if (GoldSampleSetting != null)
                {
                    GoldSampleSetting.CreateGoldSampleSelector(VariantGroups);
                }

                fromVersion = "1.0.9";
            }
            if (fromVersion == "1.0.9" || fromVersion == "1.0.10")
            {
                GoldSampleHelper.GetNewGsDataFileName(this);
                fromVersion = "1.0.11";
            }
            if (fromVersion == "1.0.11")
            {
                ServiceDesignSetting = new MTFServiceDesignSetting();
                fromVersion = "1.0.12";
            }
            if (fromVersion == "1.0.12")
            {
                ServiceDesignSetting.AllowGSPanel = true;
                fromVersion = "1.0.13";
            }
            if (fromVersion == "1.0.13")
            {
                if (properties != null && properties.ContainsKey("ExternalSubSequencesPath"))
                {
                    ExternalSubSequencesPath =
                        VersionConvertHelper.ConvertExternalSubSequencesPath(properties["ExternalSubSequencesPath"]);
                }
                fromVersion = "1.0.14";
            }
            if (fromVersion == "1.0.14")
            {
                //obsole backup setting
                fromVersion = "1.0.15";
            }
            if (fromVersion == "1.0.15")
            {
                if (properties != null && properties.ContainsKey("SequenceExecutionUiSetting"))
                {
                    var value = properties["SequenceExecutionUiSetting"];
                    var uiSetting = value as SequenceExecutionUISetting;
                    if (uiSetting == null)
                    {
                        SequenceExecutionUiSetting = new SequenceExecutionUISetting();
                    }
                }
                if (SequenceExecutionUiSetting == null)
                {
                    SequenceExecutionUiSetting = new SequenceExecutionUISetting();
                }
                fromVersion = "1.0.16";
            }
            if (fromVersion == "1.0.16")
            {
                if (properties != null && properties.ContainsKey("SequenceExecutionUiSetting"))
                {
                    var value = properties["SequenceExecutionUiSetting"];
                    var uiSetting = value as SequenceExecutionUISetting;
                    if (uiSetting == null)
                    {
                        SequenceExecutionUiSetting = new SequenceExecutionUISetting();
                    }
                }
                fromVersion = "1.0.17";
            }
            if (fromVersion == "1.0.17")
            {
                VersionConvertHelper.ConvertValidationTableResultTerms(this);
                fromVersion = "1.0.18";
            }
            if (fromVersion == "1.0.18")
            {
                VersionConvertHelper.ConvertExternalActivities(this);
                fromVersion = "1.0.19";
            }
            if (fromVersion == "1.0.19")
            {
                VersionConvertHelper.ConvertGSSetting(this);
                fromVersion = "1.0.20";
            }
            if (fromVersion == "1.0.20")
            {
                VersionConvertHelper.AddNokGS(this);
                fromVersion = "1.0.21";
            }
            if (fromVersion == "1.0.21")
            {
                SequenceVersion = VersionConvertHelper.CreateVersion(this);
                fromVersion = "1.0.22";
            }
            if (fromVersion == "1.0.22")
            {
                GraphicalViewSetting = new GraphicalViewSetting();
                fromVersion = "1.0.23";
            }
            if (fromVersion == "1.0.23")
            {
                var listActivities = new List<MTFSequenceActivity>();
                this.ForEachActivity(x =>
                                     {
                                         if (x.InternalProperties != null && x.InternalProperties.ContainsKey("AllowSwitchExecutionView") && (bool)x.InternalProperties["AllowSwitchExecutionView"])
                                         {
                                             listActivities.Add(x);
                                         }
                                     });
                VersionConvertHelper.ConvertSwitchToView(this, listActivities);
                fromVersion = "1.0.24";
            }
            if (fromVersion == "1.0.24")
            {
                ServiceDesignSetting.HideHeaderInService = false;
                ServiceDesignSetting.HideHeaderInTeach = false;
                fromVersion = "1.0.25";
            }
            if (fromVersion == "1.0.25")
            {
                VersionConvertHelper.RepairConstantTables(MTFVariables);
                fromVersion = "1.0.26";
            }
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string FullName => Name + BaseConstants.SequenceExtension;

        [IgnoreDataMember]
        public IList<MTFExternalSequenceInfo> ExternalSubSequences { get; set; }

        [IgnoreDataMember]
        public string FullPath { get; set; }

        [IgnoreDataMember]
        public bool IsLoad { get; set; }

        public Dictionary<string, bool> ExternalSubSequencesPath
        {
            get => GetProperty<Dictionary<string, bool>>();
            set => SetProperty(value);
        }

        public IList<MTFSequenceClassInfo> MTFSequenceClassInfos
        {
            get => GetProperty<IList<MTFSequenceClassInfo>>();
            set => SetProperty(value);
        }

        public IList<MTFSequenceActivity> MTFSequenceActivities
        {
            get => GetProperty<IList<MTFSequenceActivity>>();
            set => SetProperty(value);
        }

        public IList<MTFSequenceActivity> ActivitiesByCall
        {
            get => GetProperty<IList<MTFSequenceActivity>>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<MTFVariable> MTFVariables
        {
            get => GetProperty<MTFObservableCollection<MTFVariable>>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<MTFServiceCommand> ServiceCommands
        {
            get => GetProperty<MTFObservableCollection<MTFServiceCommand>>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<MTFUserCommand> UserCommands
        {
            get => GetProperty<MTFObservableCollection<MTFUserCommand>>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Mapping between components from SubSequences and components from MainSequence.
        /// Key: Guid of component in SubSequence
        /// Value: Guid of component in MainSequence
        /// </summary>
        public Dictionary<Guid, Guid> ComponetsMapping
        {
            get => GetProperty<Dictionary<Guid, Guid>>();
            set => SetProperty(value);
        }

        public bool SendEmailAfterChanged
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string SmtpServer
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string MailTo
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string EmailSubject
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public Version SequenceVersion
        {
            get => GetProperty<Version>();
            set => SetProperty(value);
        }

        public Version MTFVersion
        {
            get => GetProperty<Version>();
            set { ExecuteWithoutSetIsModified(() => SetProperty(value)); }
        }

        public IList<SequenceVariantGroup> VariantGroups
        {
            get => GetProperty<IList<SequenceVariantGroup>>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<RoundingRule> RoundingRules
        {
            get => GetProperty<MTFObservableCollection<RoundingRule>>();
            set => SetProperty(value);
        }

        public MTFServiceDesignSetting ServiceDesignSetting
        {
            get => GetProperty<MTFServiceDesignSetting>();
            set => SetProperty(value);
        }

        public SequenceExecutionUISetting SequenceExecutionUiSetting
        {
            get => GetProperty<SequenceExecutionUISetting>();
            set => SetProperty(value);
        }

        public GoldSampleSetting GoldSampleSetting
        {
            get => GetProperty<GoldSampleSetting>();
            set => SetProperty(value);
        }

        public SequenceVersionSettings VersionSettings
        {
            get => GetProperty<SequenceVersionSettings>();
            set => SetProperty(value);
        }

        public IList<SequenceChangeLog> Changes
        {
            get => GetProperty<List<SequenceChangeLog>>();
            set => SetProperty(value);
        }

        public GraphicalViewSetting GraphicalViewSetting
        {
            get => GetProperty<GraphicalViewSetting>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<DeviceUnderTestInfo> DeviceUnderTestInfos
        {
            get => GetProperty<MTFObservableCollection<DeviceUnderTestInfo>>();
            set => SetProperty(value);
        }

        public void AddChange(string modifiedByUser)
        {
            if (Changes == null)
            {
                Changes = new List<SequenceChangeLog>();
            }
            Changes.Add(new SequenceChangeLog { TimeStamp = DateTime.Now, UserName = modifiedByUser });
            if (Changes.Count > 50)
            {
                Changes.RemoveAt(0);
            }
        }

        //replace guids for non plain structure
        public void ReplaceGuids()
        {
            Id = Guid.NewGuid();
            if (MTFVariables != null)
            {
                foreach (var item in MTFVariables)
                {
                    item.Id = Guid.NewGuid();
                    if (item.HasTable && item.Value is MTFDataTransferObject dtoTable)
                    {
                        dtoTable.Id = Guid.NewGuid();
                    }
                }
            }
            if (MTFSequenceClassInfos != null)
            {
                foreach (var item in MTFSequenceClassInfos)
                {
                    var guid = Guid.NewGuid();
                    ReplaceComponentMapping(item.Id, guid, ComponetsMapping);
                    item.Id = guid;
                }
            }
            this.ForEachActivity(x => x.Id = Guid.NewGuid());
        }

        public void IncreaseRevision()
        {
            if (SequenceVersion != null)
            {
                SequenceVersion.IncreaseRevision();
            }
        }

        private static void ReplaceComponentMapping(Guid originalGuid, Guid newGuid, Dictionary<Guid, Guid> componentsMapping)
        {
            if (componentsMapping.ContainsValue(originalGuid))
            {
                var items = componentsMapping.Where(x => x.Value == originalGuid);
                foreach (var item in items.ToList())
                {
                    componentsMapping[item.Key] = newGuid;
                }
            }
        }
    }

    [Serializable]
    public class SequenceChangeLog : MTFDataTransferObject
    {
        public SequenceChangeLog()
        {
        }

        public SequenceChangeLog(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string UserName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public DateTime TimeStamp
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value);
        }
    }

}
