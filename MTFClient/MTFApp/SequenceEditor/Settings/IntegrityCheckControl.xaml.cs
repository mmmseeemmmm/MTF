using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.PopupWindow;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for IntegrityCheckControl.xaml
    /// </summary>
    public partial class IntegrityCheckControl : UserControl, IRaiseCloseEvent, INotifyPropertyChanged
    {
        private readonly MTFSequence mainSequence;
        private readonly IList<MTFSequence> externalSequences;
        private readonly ObservableCollection<IntegrityCheckInfo> activities = new ObservableCollection<IntegrityCheckInfo>();
        private readonly ObservableCollection<IntegrityCheckInfo> wholeSequenceCalls = new ObservableCollection<IntegrityCheckInfo>();
        private readonly Dictionary<Guid, List<ExternalActivityInfo>> availableSubSequences = new Dictionary<Guid, List<ExternalActivityInfo>>();

        public IntegrityCheckControl(MTFSequence mainSequence, IList<MTFSequence> externalSequences)
        {
            this.mainSequence = mainSequence;
            this.externalSequences = externalSequences;
            InitializeComponent();
            IntegrityCheckControlRoot.DataContext = this;
            ProcessSequence(mainSequence, externalSequences);

        }

        public ObservableCollection<IntegrityCheckInfo> Activities => activities;

        public ObservableCollection<IntegrityCheckInfo> WholeSequenceCalls => wholeSequenceCalls;

        public bool HasWholeCalls => wholeSequenceCalls != null && wholeSequenceCalls.Count > 0;

        public bool HasMissingActivities => activities != null && activities.Count > 0;

        private void InvalidateCollections()
        {
            NotifyPropertyChanged("HasWholeCalls");
            NotifyPropertyChanged("HasMissingActivities");
        }

        private async void ProcessSequence(MTFSequence main, IList<MTFSequence> external)
        {
            await Task.Run(() =>
                     {
                         if (main != null && external != null && external.Count > 0)
                         {
                             mainSequence.ForEachActivity<MTFExecuteActivity>(x => CheckExternalCallIntegrity(x, external));
                         }
                     });
            UIHelper.InvokeOnDispatcherAsync(InvalidateCollections);
        }

        private void CheckExternalCallIntegrity(MTFExecuteActivity executeActivity, IList<MTFSequence> externalSequences)
        {
            if (executeActivity.Type == ExecuteActyvityTypes.External)
            {
                var call = executeActivity.ExternalCall;
                if (call != null)
                {
                    var brokenSequence = externalSequences.FirstOrDefault(x => x.Name == call.ExternalSequenceToCall);
                    if (brokenSequence != null)
                    {

                        var callCollection = brokenSequence.ActivitiesByCall.Select(x => x.Id).ToList();
                        if (!callCollection.Contains(call.InnerSubSequenceByCallId))
                        {
                            var info = new IntegrityCheckInfo
                            {
                                Activity = executeActivity,
                                SequenceName = call.ExternalSequenceToCall,
                                ExternalCall = new ExternalCallInfo(),
                            };

                            if (!availableSubSequences.ContainsKey(brokenSequence.Id))
                            {
                                availableSubSequences[brokenSequence.Id] = brokenSequence.ActivitiesByCall.Select(
                                x => new ExternalActivityInfo
                                {
                                    Id = x.Id,
                                    OriginalName = x.ActivityName,
                                    TranslatedName = TranslateName(x.ActivityName, x.UniqueIndexer),
                                    UniqueIndexer = x.UniqueIndexer
                                }).ToList();
                                availableSubSequences[brokenSequence.Id].Insert(0, new ExternalActivityInfo
                                                                                   {
                                                                                       Id = ActivityNameConstants.CallWholeSequenceId,
                                                                                       OriginalName = ActivityNameConstants.CallWholeSequenceKey,
                                                                                       TranslatedName = TranslateName(ActivityNameConstants.CallWholeSequenceKey, 0),
                                                                                       UniqueIndexer = 0,
                                                                                   });
                            }

                            info.ExternalCall.AvailableSubSequences = availableSubSequences[brokenSequence.Id];

                            if (call.InnerSubSequenceByCallId != ActivityNameConstants.CallWholeSequenceId)
                            {
                                var callActivities = brokenSequence.ActivitiesByCall.Where(x => x.ActivityName == call.OriginalCallActivityName).ToList();
                                info.IsAmbiguous = callActivities.Count != 1;

                                if (callActivities.Count > 0)
                                {
                                    info.ExternalCall.InnerSubSequenceByCallId = callActivities.First().Id;
                                }

                                UIHelper.InvokeOnDispatcherAsync(() => activities.Add(info));
                            }
                            else
                            {
                                info.IsWhole = true;
                                info.ExternalCall.InnerSubSequenceByCallId = info.Activity.ExternalCall.InnerSubSequenceByCallId;
                                UIHelper.InvokeOnDispatcherAsync(() => wholeSequenceCalls.Add(info));
                            }
                        }
                    }
                }
            }
        }


        public ICommand ApplyCommand => new Command(Apply);

        private void Apply()
        {
            if (activities != null)
            {
                foreach (var integrityCheckInfo in activities)
                {
                    integrityCheckInfo.Activity.ExternalCall.InnerSubSequenceByCallId = integrityCheckInfo.ExternalCall.InnerSubSequenceByCallId;
                }
                activities.Clear();
               
            }
            if (wholeSequenceCalls!=null)
            {
                foreach (var integrityCheckInfo in wholeSequenceCalls)
                {
                    integrityCheckInfo.Activity.ExternalCall.InnerSubSequenceByCallId = integrityCheckInfo.ExternalCall.InnerSubSequenceByCallId;
                }
                wholeSequenceCalls.Clear();
            }
            ProcessSequence(mainSequence, externalSequences);

        }

        public ICommand CancelCommand => new Command(Cancel);

        private void Cancel()
        {
            Close?.Invoke(this);
        }

        private static string TranslateName(string key, int indexer)
        {
            return MTFSequenceActivityHelper.CombineTranslatedActivityName(SequenceLocalizationHelper.ActualDictionary.GetValue(key), indexer);
        }

        public event CloseEventHandler Close;

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
    }


    public class IntegrityCheckInfo
    {
        public MTFExecuteActivity Activity { get; set; }
        public string SequenceName { get; set; }
        public ExternalCallInfo ExternalCall { get; set; }
        public bool IsAmbiguous { get; set; }
        public bool IsWhole { get; set; }

        public string TargetName => !IsWhole ? Activity.ExternalCall.OriginalCallActivityName : string.Empty;
    }
}
