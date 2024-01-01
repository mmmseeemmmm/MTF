using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.SequenceEditor.DetailDataTemplates
{
    /// <summary>
    /// Interaction logic for ExecuteActivityDetail.xaml
    /// </summary>
    public partial class ExecuteActivityDetail : ActivityDetailBase
    {
        public ExecuteActivityDetail()
        {
            InitializeComponent();
        }

        public SequenceEditorPresenter Presenter
        {
            get { return (SequenceEditorPresenter)GetValue(PresenterProperty); }
            set { SetValue(PresenterProperty, value); }
        }

        public static readonly DependencyProperty PresenterProperty =
            DependencyProperty.Register("Presenter", typeof(SequenceEditorPresenter), typeof(ExecuteActivityDetail),
                new FrameworkPropertyMetadata());


        private void AvailableSubsequencesComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;
            if (combo.Items.Count > 0 && e.AddedItems.Count > 0)
            {
                var activity = DataContext as MTFExecuteActivity;
                if (activity != null && Presenter != null && activity.ExternalCall != null)
                {
                    var externalSequence = combo.SelectedItem as MTFSequence;
                    var loadedSequence = externalSequence;
                    if (externalSequence != null && !externalSequence.IsLoad)
                    {
                        loadedSequence = Presenter.LoadForExecuteActivity(externalSequence);
                    }

                    if (loadedSequence != null && loadedSequence.ActivitiesByCall != null)
                    {
                        var tmp =
                            loadedSequence.ActivitiesByCall.Select(
                                x => new ExternalActivityInfo
                                {
                                    Id = x.Id,
                                    OriginalName = x.ActivityName,
                                    TranslatedName = TranslateName(x.ActivityName, x.UniqueIndexer),
                                    UniqueIndexer = x.UniqueIndexer
                                }).ToList();
                        tmp.Insert(0,
                            new ExternalActivityInfo
                            {
                                Id = ActivityNameConstants.CallWholeSequenceId,
                                OriginalName = ActivityNameConstants.CallWholeSequenceKey,
                                TranslatedName = TranslateName(ActivityNameConstants.CallWholeSequenceKey, 0),
                                UniqueIndexer = 0,
                            });

                        activity.ExternalCall.AvailableSubSequences = tmp;
                    }

                    if (activity.ExternalCall.InnerSubSequenceByCallId != Guid.Empty)
                    {
                        activity.InvalidateExternalCall();
                    }
                }
            }
        }

        private static string TranslateName(string key, int indexer)
        {
            return MTFSequenceActivityHelper.CombineTranslatedActivityName(SequenceLocalizationHelper.ActualDictionary.GetValue(key), indexer);
        }
    }
}