using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.FindUsages
{


    /// <summary>
    /// Interaction logic for FindUsagesWindow.xaml
    /// </summary>
    public partial class FindVariableUsagesWindow : FindUsagesBase
    {
        private readonly ObservableCollection<FindVariablesDataContainer> items = new ObservableCollection<FindVariablesDataContainer>();
        private readonly MTFVariable variable;
        private readonly Command refreshCommand;
        private bool isLoading;

        public FindVariableUsagesWindow(FindUsagesSetting setting, MTFVariable variable)
            : base(setting)
        {
            InitializeComponent();
            Root.DataContext = this;
            Title = variable.Name;
            this.variable = variable;
            refreshCommand = new Command(Refresh, () => !isLoading);
            StartFindUsagesAsync();
        }



        public ObservableCollection<FindVariablesDataContainer> Items
        {
            get { return items; }
        }

        public override ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        private void Refresh()
        {
            items.Clear();
            NotifyPropertyChanged("GetItems");
            NotifyPropertyChanged("SetItems");
            StartFindUsagesAsync();
        }

        protected override void InvalidateButtons()
        {
            base.InvalidateButtons();
            NotifyPropertyChanged("GetIsSelected");
            NotifyPropertyChanged("SetIsSelected");
        }

        public IEnumerable<FindVariablesDataContainer> GetItems
        {
            get { return Items.Where(x => !x.IsSet); }
        }

        public IEnumerable<FindVariablesDataContainer> SetItems
        {
            get { return Items.Where(x => x.IsSet); }
        }

        public MTFVariable Variable
        {
            get { return variable; }
        }

        public bool SetIsSelected
        {
            get { return DisplayMode == DisplayModes.Set; }
        }

        public bool GetIsSelected
        {
            get { return DisplayMode == DisplayModes.Get; }
        }

        private async void StartFindUsagesAsync()
        {
            Owner.IsEnabled = false;
            isLoading = true;
            refreshCommand.RaiseCanExecuteChanged();
            if (Setting.Sequence == null || variable == null)
            {
                return;
            }
            await Task.Run(() => Setting.Sequence.ForEachActivity(x => CheckActivity(x, Setting.Sequence)));
            if (IncludeExternal)
            {
                var allSequences = Setting.GetAllSequences();
                foreach (var mtfSequence in allSequences)
                {
                    if (mtfSequence != Setting.Sequence)
                    {
                        var currentSequence = mtfSequence;
                        await Task.Run(() => currentSequence.ForEachActivity(x => CheckActivity(x, currentSequence)));
                    }
                }
            }
            isLoading = false;
            refreshCommand.RaiseCanExecuteChanged();
            if (DisplayMode == DisplayModes.Get)
            {
                NotifyPropertyChanged("GetItems");
            }
            if (DisplayMode == DisplayModes.Set)
            {
                NotifyPropertyChanged("SetItems");
            }
            Owner.IsEnabled = true;
        }

        private void CheckActivity(MTFSequenceActivity activity, MTFSequence currentSequence)
        {
            const string type = "Function parameter";
            var setVariable = activity as MTFVariableActivity;
            if (setVariable != null)
            {
                ProcessSetVariableActivity(setVariable, currentSequence);
            }
            else if (activity is MTFSequenceHandlingActivity)
            {
                ProcessSequenceHandlingActivity((MTFSequenceHandlingActivity)activity, currentSequence);
            }
            else if (activity is MTFErrorHandlingActivity)
            {
                ProcessErrorHandlingActivity((MTFErrorHandlingActivity)activity, currentSequence);
            }
            else if (activity is MTFSequenceMessageActivity)
            {
                ProcessMessageActivity((MTFSequenceMessageActivity)activity, currentSequence);
            }
            else
            {
                if (activity.MTFParameters != null)
                {
                    foreach (var mtfParameterValue in activity.MTFParameters)
                    {
                        var termValue = mtfParameterValue.Value as Term;
                        if (termValue != null)
                        {
                            ProcessTerm(termValue, activity, type, currentSequence);
                        }
                        var collection = mtfParameterValue.Value as ICollection;
                        if (collection != null)
                        {
                            foreach (var item in collection)
                            {
                                ProcessGenericClassInstaceConfiguration(item as GenericClassInstanceConfiguration, activity, type, currentSequence);
                                ProcessTerm(item as Term, activity, type, currentSequence);
                            }
                        }

                    }
                }

            }

            if (activity.Term != null)
            {
                ProcessTerm(activity.Term, activity, activity is MTFSubSequenceActivity ? "Condition" : "Check output value", currentSequence);
            }
            if (activity.Variable != null && activity.Variable.Name == variable.Name && !(activity is MTFVariableActivity))
            {
                AddNewDataContainer(new FindVariablesDataContainer(activity, null, true, "Assign output value", currentSequence));
            }
            if (activity.ErrorOutput == variable)
            {
                AddNewDataContainer(new FindVariablesDataContainer(activity, null, true, "Append error", currentSequence));
            }
        }

        private void ProcessMessageActivity(MTFSequenceMessageActivity messageActivity, MTFSequence currentSequence)
        {
            if (messageActivity.Header != null)
            {
                ProcessStringFormat(messageActivity.Header, messageActivity, "Header", currentSequence);
            }
            if (messageActivity.Message != null)
            {
                ProcessStringFormat(messageActivity.Message, messageActivity, "Message", currentSequence);
            }
        }

        private void ProcessErrorHandlingActivity(MTFErrorHandlingActivity errorHandlingActivity, MTFSequence currentSequence)
        {
            if (errorHandlingActivity.RaiseError != null)
            {
                ProcessStringFormat(errorHandlingActivity.RaiseError, errorHandlingActivity, "Error message", currentSequence);
            }
        }

        private void ProcessGenericClassInstaceConfiguration(GenericClassInstanceConfiguration gci, MTFSequenceActivity activity, string type, MTFSequence currentSequence)
        {
            if (gci != null)
            {
                foreach (var propertyValue in gci.PropertyValues)
                {
                    var termVariable = propertyValue.Value as Term;
                    if (termVariable != null)
                    {
                        ProcessTerm(termVariable, activity, type, currentSequence);
                    }

                }
            }
        }

        private void ProcessTerm(Term term, MTFSequenceActivity activity, string type, MTFSequence currentSequence)
        {
            if (term == null)
            {
                return;
            }
            var termWrapper = term as TermWrapper;
            if (termWrapper != null)
            {
                ProcessGenericClassInstaceConfiguration(termWrapper.Value as GenericClassInstanceConfiguration, activity, type, currentSequence);
            }
            term.ForEachTerm<VariableTerm>(x =>
            {
                if (x.MTFVariable != null && x.MTFVariable.Name == variable.Name)
                {
                    AddNewDataContainer(new FindVariablesDataContainer(activity, term, false, type, currentSequence));
                }
            });
            if (variable.HasValidationTable)
            {
                term.ForEachTerm<ValidationTableTerm>(
                        x =>
                        {
                            if (x.ValidationTable != null && x.ValidationTable.Name == ((MTFValidationTable)variable.Value).Name)
                            {
                                AddNewDataContainer(new FindVariablesDataContainer(activity, term, true, type, currentSequence));
                            }
                        });

            }
            if (variable.HasTable || variable.HasValidationTable)
            {
                term.ForEachTerm<ValidationTableResultTerm>(
                        x =>
                        {
                            if (x.ValidationTable != null && x.ValidationTable.Name == ((IMTFTable)variable.Value).Name)
                            {
                                AddNewDataContainer(new FindVariablesDataContainer(activity, term, true, type, currentSequence));
                            }
                        });
            }
        }

        private void ProcessStringFormat(MTFStringFormat stringFormat, MTFSequenceActivity activity, string type, MTFSequence currentSequence)
        {
            if (stringFormat.Parameters != null)
            {
                foreach (var term in stringFormat.Parameters)
                {
                    ProcessTerm(term, activity, type, currentSequence);
                }
            }
        }

        private void ProcessSequenceHandlingActivity(MTFSequenceHandlingActivity handlingActivity, MTFSequence currentSequence)
        {
            if (handlingActivity.Logs != null)
            {
                foreach (var stringFormat in handlingActivity.Logs)
                {
                    ProcessStringFormat(stringFormat, handlingActivity, "Logs", currentSequence);
                }
            }
            if (handlingActivity.StatusLines != null)
            {
                foreach (var stringFormat in handlingActivity.StatusLines)
                {
                    ProcessStringFormat(stringFormat, handlingActivity, "Status lines", currentSequence);
                }
            }
            if (handlingActivity.LogMessage != null)
            {
                ProcessStringFormat(handlingActivity.LogMessage, handlingActivity, "Log message", currentSequence);
            }
            if (handlingActivity.SequenceVariant != null)
            {
                foreach (var groupValue in handlingActivity.SequenceVariant.VariantGroups)
                {
                    if (groupValue.Term != null)
                    {
                        ProcessTerm(groupValue.Term, handlingActivity, "Sequence Variant", currentSequence);
                    }
                }
            }
        }

        private void ProcessSetVariableActivity(MTFVariableActivity variableActivity, MTFSequence currentSequence)
        {
            if (variableActivity.Variable != null && variableActivity.Variable.Name == variable.Name)
            {
                AddNewDataContainer(new FindVariablesDataContainer(variableActivity, null, true, "Variable", currentSequence));
            }
            if (variableActivity.Value != null)
            {
                ProcessTerm(variableActivity.Value, variableActivity, "Value", currentSequence);
            }
        }

        private void AddNewDataContainer(FindVariablesDataContainer findVariablesData)
        {
            Dispatcher.Invoke(() => items.Add(findVariablesData));
        }

        private void ItemsListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var container = listBox.SelectedItem as FindVariablesDataContainer;
                if (container != null && container.Activity != null)
                {
                    Setting.SelectionCallBack(container.Activity, container.Sequence);
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            base.MinimizeButtonClick(sender, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            base.CloseButtonClick(sender, e);
        }

        private void ItemsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                ItemsListBox_OnMouseDoubleClick(sender, null);
            }
        }
    }

    public class FindVariablesDataContainer : NotifyPropertyBase
    {
        private MTFSequenceActivity activity;
        private Term term;
        private bool isSet;
        private string propertyName;
        private MTFSequence sequence;

        public MTFSequenceActivity Activity
        {
            get { return activity; }
            set { activity = value; }
        }

        public Term Term
        {
            get { return term; }
            set { term = value; }
        }

        public bool IsSet
        {
            get { return isSet; }
            set { isSet = value; }
        }

        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }

        public MTFSequence Sequence
        {
            get { return sequence; }
            set { sequence = value; }
        }

        public FindVariablesDataContainer(MTFSequenceActivity activity, Term term, bool isSet, string propertyName, MTFSequence sequence)
        {
            this.activity = activity;
            this.term = term;
            this.isSet = isSet;
            this.propertyName = propertyName;
            this.sequence = sequence;
        }
    }
}
