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
    /// Interaction logic for FindComaponentUsagesWindow.xaml
    /// </summary>
    public sealed partial class FindComponentUsagesWindow : FindUsagesBase, IFindActivityUsages
    {
        private readonly IEnumerable<Guid> externalComponents;
        private readonly MTFSequenceClassInfo classInfo;
        private readonly ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>> items = new ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>>();
        private readonly Command refreshCommand;
        private bool isLoading;
        private string selectedMethod;
        private readonly HashSet<string> methods = new HashSet<string>();

        #region ctor

        public FindComponentUsagesWindow(FindUsagesSetting setting, MTFSequenceClassInfo classInfo, IEnumerable<Guid> externalComponents)
            : base(setting)
        {
            InitializeComponent();
            Root.DataContext = this;
            Title = classInfo.Alias;
            MTFIcon = classInfo.MTFClass.Icon;
            this.externalComponents = externalComponents;
            this.classInfo = classInfo;
            refreshCommand = new Command(Refresh, () => !isLoading);
            StartFindUsagesAsync();
        }

        public FindComponentUsagesWindow(FindUsagesSetting setting, MTFSequenceClassInfo classInfo, IEnumerable<Guid> externalComponents, string selectedMethod)
            : this(setting, classInfo, externalComponents)
        {
            this.selectedMethod = selectedMethod;
            DisplayMode = DisplayModes.Filtred;
            InvalidateButtons();
        }

        #endregion

        
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

        public ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>> Items
        {
            get { return items; }
        }

        public string SelectedType
        {
            get { return selectedMethod; }
            set
            {
                selectedMethod = value;
                if (value != null)
                {
                    DisplayMode = DisplayModes.Filtred;
                    InvalidateButtons();
                    NotifyPropertyChanged("FiltredItems");
                }
            }
        }

        public IEnumerable<string> Types
        {
            get { return methods.ToList(); }
        }

        public IEnumerable<Tuple<MTFSequenceActivity, MTFSequence>> FiltredItems
        {
            get { return Items.Where(x => x.Item1.MTFMethodDisplayName == selectedMethod); }
        }

        public MTFSequenceClassInfo ClassInfo
        {
            get { return classInfo; }
        }

        private async void StartFindUsagesAsync()
        {
            Owner.IsEnabled = false;
            isLoading = true;
            refreshCommand.RaiseCanExecuteChanged();
            if (Setting.Sequence != null && classInfo != null)
            {
                await Task.Run(() => Setting.Sequence.ForEachActivity(CheckActivity));
                if (IncludeExternal)
                {
                    var componentsGuids = externalComponents.ToList();
                    if (componentsGuids.Count > 0)
                    {
                        var allSequences = Setting.GetAllSequences();
                        foreach (var mtfSequence in allSequences)
                        {
                            if (mtfSequence != Setting.Sequence)
                            {
                                var currentSequence = mtfSequence;
                                await
                                    Task.Run(
                                        () => currentSequence.ForEachActivity(x => CheckExternalActivity(x, componentsGuids, currentSequence)));
                            }
                        }
                    }
                }
            }
            isLoading = false;
            refreshCommand.RaiseCanExecuteChanged();
            NotifyPropertyChanged("Types");
            if (DisplayMode == DisplayModes.Filtred)
            {
                NotifyPropertyChanged("FiltredItems");
            }
            Owner.IsEnabled = true;
        }

        private void CheckActivity(MTFSequenceActivity activity)
        {
            if (activity.ClassInfo == classInfo)
            {
                AddNewItem(activity, Setting.Sequence);
            }
        }

        private void CheckExternalActivity(MTFSequenceActivity activity, List<Guid> componentsGuids, MTFSequence currentSequence)
        {
            if (activity.ClassInfo != null && componentsGuids.Contains(activity.ClassInfo.Id))
            {
                AddNewItem(activity, currentSequence);
            }
        }

        private void AddNewItem(MTFSequenceActivity activity, MTFSequence currentSequence)
        {
            Dispatcher.Invoke(() =>
                              {
                                  items.Add(new Tuple<MTFSequenceActivity, MTFSequence>(activity, currentSequence));
                                  if (!string.IsNullOrEmpty(activity.MTFMethodDisplayName))
                                  {
                                      methods.Add(activity.MTFMethodDisplayName);
                                  }
                              });
        }

        public MTFIcons MTFIcon { get; set; }
        
    }
}
