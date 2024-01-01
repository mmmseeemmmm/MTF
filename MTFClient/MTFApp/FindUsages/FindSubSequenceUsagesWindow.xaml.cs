using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.FindUsages
{
    /// <summary>
    /// Interaction logic for FindSubSequenceUsagesWindow.xaml
    /// </summary>
    public partial class FindSubSequenceUsagesWindow : FindUsagesBase
    {
        private readonly MTFSequenceActivity activityToFind;
        private readonly ObservableCollection<FindSubSequencesDataContainer> items = new ObservableCollection<FindSubSequencesDataContainer>();
        private readonly Command refreshCommand;
        private bool isLoading;
        private FindSubSequenceUsagesType selectedType;
        private Action<MTFServiceCommand, MTFSequence> SelectCommandCallBack { get; set; }

        public FindSubSequenceUsagesWindow(FindUsagesSetting setting, MTFSequenceActivity activityToFind, Action<MTFServiceCommand, MTFSequence> selectCommandCallBack)
            : base(setting)
        {
            this.activityToFind = activityToFind;
            SelectCommandCallBack = selectCommandCallBack;
            InitializeComponent();
            Root.DataContext = this;
            Title = activityToFind.ActivityName;
            DisplayMode = DisplayModes.All;
            refreshCommand = new Command(Refresh, () => !isLoading);
            StartFindUsagesAsync();
        }

        private async void StartFindUsagesAsync()
        {
            Owner.IsEnabled = false;
            isLoading = true;
            refreshCommand.RaiseCanExecuteChanged();
            if (Setting.Sequence == null)
            {
                return;
            }
            await Task.Run(() => Setting.Sequence.ForEachActivity(x => CheckActivity(x, Setting.Sequence)));
            await Task.Run(() => CheckCommad(Setting.Sequence.ServiceCommands, Setting.Sequence));
            if (IncludeExternal)
            {
                var allSequences = Setting.GetAllSequences();
                foreach (var mtfSequence in allSequences)
                {
                    if (mtfSequence != Setting.Sequence)
                    {
                        var currentSequence = mtfSequence;
                        await Task.Run(() => currentSequence.ForEachActivity(x => CheckActivity(x, currentSequence)));
                        await Task.Run(() => CheckCommad(currentSequence.ServiceCommands, currentSequence));
                    }
                }
            }
            isLoading = false;
            refreshCommand.RaiseCanExecuteChanged();
            Owner.IsEnabled = true;
        }

        public override ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        
        public FindSubSequenceUsagesType SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                DisplayMode = DisplayModes.Filtred;
                InvalidateButtons();
                NotifyPropertyChanged("FiltredItems");
            }
        }

        private void CheckCommad(IEnumerable<MTFServiceCommand> commands, MTFSequence sequence)
        {
            if (commands == null)
            {
                return;
            }
            foreach (var command in commands)
            {
                bool add = false;
                string location = string.Empty;
                if (command.PrepairActivity == activityToFind)
                {
                    add = true;
                    location = "Prepare";
                }
                else if (command.ExecuteActivity == activityToFind)
                {
                    add = true;
                    location = "Execute";
                }
                else if (command.CleaunupActivity == activityToFind)
                {
                    add = true;
                    location = "Clean up";
                }
                if (add)
                {
                    var data = new FindSubSequencesDataContainer(command, command.Name, location, sequence, FindSubSequenceUsagesType.Command);
                    AddNewDataContainer(data);
                }

            }
        }

        private void CheckActivity(MTFSequenceActivity activity, MTFSequence sequence)
        {
            bool add = false;
            var location = string.Empty;
            var callActivity = activity as MTFExecuteActivity;
            if (callActivity != null)
            {
                switch (callActivity.Type)
                {
                    case ExecuteActyvityTypes.Local:
                        add = callActivity.ActivityToCall == activityToFind;
                        break;
                    case ExecuteActyvityTypes.External:
                        if (callActivity.ExternalCall!=null && callActivity.ExternalCall.InnerSubSequenceByCallId == activityToFind.Id)
                        {
                            var parentSequence = activityToFind.GetParent<MTFSequence>();
                            add = parentSequence != null && callActivity.ExternalCall != null && parentSequence.Name == callActivity.ExternalCall.ExternalSequenceToCall;
                        }
                        break;
                }
            }
            else
            {
                var subSequence = activity as MTFSubSequenceActivity;
                if (subSequence != null)
                {
                    if (subSequence.PrepaireServiceActivity == activityToFind)
                    {
                        add = true;
                        location = "Prepare service";
                    }
                    if (subSequence.CleanupServiceActivity == activityToFind)
                    {
                        add = true;
                        location = "Cleanup service";
                    }
                }
            }
            if (add)
            {
                var data = new FindSubSequencesDataContainer(activity, activity.TranslateActivityName(), location, sequence, FindSubSequenceUsagesType.Activity);
                AddNewDataContainer(data);
            }
        }

        private void AddNewDataContainer(FindSubSequencesDataContainer data)
        {
            Dispatcher.Invoke(() => items.Add(data));
        }

        public ObservableCollection<FindSubSequencesDataContainer> Items
        {
            get { return items; }
        }

        public IEnumerable<FindSubSequencesDataContainer> FiltredItems
        {
            get { return Items.Where(x => x.Type == selectedType); }
        }

        private void Refresh()
        {
            items.Clear();
            NotifyPropertyChanged("FiltredItems");
            StartFindUsagesAsync();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            base.MinimizeButtonClick(sender, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            base.CloseButtonClick(sender, e);
        }

        private void ItemsListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var data = listBox.SelectedItem as FindSubSequencesDataContainer;
                if (data != null)
                {
                    var activity = data.Item as MTFSequenceActivity;
                    if (activity != null)
                    {
                        Setting.SelectionCallBack(activity, data.Sequence);
                        return;
                    }
                    var command = data.Item as MTFServiceCommand;
                    if (command != null)
                    {
                        SelectCommandCallBack(command, data.Sequence);
                    }
                }
            }
        }

        private void ItemsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                ItemsListBox_OnMouseDoubleClick(sender, null);
            }
        }

        public IEnumerable<EnumValueDescription> FindSubSequenceUsagesTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<FindSubSequenceUsagesType>(); }
        }
    }

    public class FindSubSequencesDataContainer
    {
        private MTFDataTransferObject item;
        private string location;
        private MTFSequence sequence;
        private string name;
        private FindSubSequenceUsagesType type;

        public FindSubSequencesDataContainer(MTFDataTransferObject item, string name, string location, MTFSequence sequence, FindSubSequenceUsagesType type)
        {
            this.item = item;
            this.location = location;
            this.sequence = sequence;
            this.type = type;
            this.name = name;
        }

        public MTFDataTransferObject Item
        {
            get { return item; }
            set { item = value; }
        }

        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public MTFSequence Sequence
        {
            get { return sequence; }
            set { sequence = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public FindSubSequenceUsagesType Type
        {
            get { return type; }
            set { type = value; }
        }
    }

    public enum FindSubSequenceUsagesType
    {
        [Description("Activity")]
        Activity,
        [Description("Command")]
        Command
    }
}
