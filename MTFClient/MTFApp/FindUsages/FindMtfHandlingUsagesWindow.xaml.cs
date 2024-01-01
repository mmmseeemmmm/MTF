using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutomotiveLighting.MTFCommon;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.FindUsages
{
    /// <summary>
    /// Interaction logic for FindMtfHandlingUsagesWindow.xaml
    /// </summary>
    public partial class FindMtfHandlingUsagesWindow : FindUsagesBase, IFindActivityUsages
    {
        private readonly Type typeToFind;
        private readonly Command refreshCommand;
        private readonly ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>> items = new ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>>();
        private bool isLoading;
        private string selectedType;
        private readonly HashSet<string> types = new HashSet<string>();

        public FindMtfHandlingUsagesWindow(FindUsagesSetting setting, Type typeToFind, MTFIcons icon)
            : base(setting)
        {
            InitializeComponent();
            Root.DataContext = this;
            this.typeToFind = typeToFind;
            Title = this.typeToFind.Name;
            MTFIcon = icon;
            refreshCommand = new Command(Refresh, () => !isLoading);
            StartFindUsagesAsync();
        }

        public override ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        private void Refresh()
        {
            items.Clear();
            NotifyPropertyChanged("FiltredItems");
            StartFindUsagesAsync();
        }

        public IEnumerable<string> Types
        {
            get { return types.ToList(); }
        }

        public ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>> Items
        {
            get { return items; }
        }

        public IEnumerable<Tuple<MTFSequenceActivity, MTFSequence>> FiltredItems
        {
            get { return Items.Where(x => GetHandlingType(x.Item1) == selectedType); }
        }

        private string GetHandlingType(MTFSequenceActivity activity)
        {
            var sequenceHandlingActivity = activity as MTFSequenceHandlingActivity;
            if (sequenceHandlingActivity != null)
            {
                return sequenceHandlingActivity.SequenceHandlingType.ToString();
            }

            var errorHandlingActivity = activity as MTFErrorHandlingActivity;
            if (errorHandlingActivity != null)
            {
                return errorHandlingActivity.ErrorHandlingType.ToString();
            }

            var messageActivity = activity as MTFSequenceMessageActivity;
            if (messageActivity != null)
            {
                return messageActivity.MessageType.ToString();
            }

            return null;
        }

        public string SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                if (value != null)
                {
                    DisplayMode = DisplayModes.Filtred;
                    InvalidateButtons();
                    NotifyPropertyChanged("FiltredItems");
                }
            }
        }

        private async void StartFindUsagesAsync()
        {
            Owner.IsEnabled = false;
            isLoading = true;
            isLoading = false;
            refreshCommand.RaiseCanExecuteChanged();
            if (Setting.Sequence != null)
            {
                await Task.Run(() => Setting.Sequence.ForEachActivity(x => CheckActivity(x, Setting.Sequence)));
                if (IncludeExternal)
                {
                    var allSequences = Setting.GetAllSequences();
                    foreach (var mtfSequence in allSequences)
                    {
                        if (mtfSequence != Setting.Sequence)
                        {
                            var currentSequence = mtfSequence;
                            await
                                Task.Run(
                                    () => currentSequence.ForEachActivity(x => CheckActivity(x, currentSequence)));
                        }
                    }
                }
            }

            refreshCommand.RaiseCanExecuteChanged();
            NotifyPropertyChanged("Types");
            if (DisplayMode == DisplayModes.Filtred)
            {
                NotifyPropertyChanged("FiltredItems");
            }
            Owner.IsEnabled = true;
        }

        private void CheckActivity(MTFSequenceActivity activity, MTFSequence sequence)
        {
            if (activity.GetType() == typeToFind)
            {
                AddNewItem(activity, sequence);
            }
        }

        private void AddNewItem(MTFSequenceActivity activity, MTFSequence currentSequence)
        {
            Dispatcher.Invoke(() =>
            {
                items.Add(new Tuple<MTFSequenceActivity, MTFSequence>(activity, currentSequence));
                var handlingType = GetHandlingType(activity);
                if (!string.IsNullOrEmpty(handlingType))
                {
                    types.Add(handlingType);
                }
            });
        }


        public MTFIcons MTFIcon { get; set; }
    }
}
