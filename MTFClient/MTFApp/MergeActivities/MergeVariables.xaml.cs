using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.MergeActivities
{
    /// <summary>
    /// Interaction logic for MergeVariables.xaml
    /// </summary>
    public partial class MergeVariables : MergeActivitiesBase
    {
        private readonly MTFObservableCollection<MTFVariable> sequenceVariables;
        private readonly MergeSharedData mergedData;
        private bool hasTable;

        public MergeVariables(MTFObservableCollection<MTFVariable> sequenceVariables, MergeSharedData mergedData)
        {
            InitializeComponent();
            this.sequenceVariables = sequenceVariables;
            this.mergedData = mergedData;
            this.Root.DataContext = this;
            LoadVariablesAsync();
        }

        private async void LoadVariablesAsync()
        {
            List<MTFVariable> mergedVariables = null;
            await Task.Run(() => mergedVariables = GetAllVariables(mergedData.AllActivities).Distinct().ToList());
            await Task.Run(() => mergedData.VariablesMapping = TransformToMergedData(mergedVariables, mergedData));
            NotifyPropertyChanged("HasTable");
            NotifyPropertyChanged("CurrentVariables");
        }

        private IEnumerable<MergeActivitiesData<MTFVariable>> TransformToMergedData(List<MTFVariable> variables, MergeSharedData data)
        {
            var variablesMapping = new ObservableCollection<MergeActivitiesData<MTFVariable>>();
            if (variables != null)
            {
                foreach (var mtfVariable in variables)
                {
                    var variablesData = new MergeActivitiesData<MTFVariable>
                                        {
                                            OriginalData = mtfVariable,
                                            AllowedData = sequenceVariables.Where(v => v.TypeName == mtfVariable.TypeName).ToList()
                                        };
                    variablesData.AllowedData.Add(new NewVariable());
                    variablesMapping.Add(variablesData);
                    variablesData.NewData = ChooseSelectedItem(variablesData.AllowedData, mtfVariable.Name);
                }
            }
            return variablesMapping;
        }

        private MTFVariable ChooseSelectedItem(IList<MTFVariable> allowedData, string name)
        {
            var variable = allowedData.FirstOrDefault(x => x.Name == name);
            return variable ?? allowedData.LastOrDefault();
        }

        private IEnumerable<MTFVariable> GetAllVariables(IEnumerable<MTFSequenceActivity> activities)
        {
            var output = new List<MTFVariable>();
            foreach (var activity in activities)
            {
                var setVariable = activity as MTFVariableActivity;
                if (setVariable != null)
                {
                    output.AddRange(GetAllVariables(setVariable));
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
                                output.AddRange(GetAllVariables(termValue));
                            }
                        }
                    }
                    if (activity.Term != null)
                    {
                        output.AddRange(GetAllVariables(activity.Term));
                    }
                }
            }
            return output;
        }

        private IEnumerable<MTFVariable> GetAllVariables(MTFVariableActivity variableActivity)
        {
            var output = new List<MTFVariable>();
            if (variableActivity.Variable != null)
            {
                output.Add(variableActivity.Variable);
            }
            if (variableActivity.Value != null)
            {
                output.AddRange(GetAllVariables(variableActivity.Value));
            }
            return output;
        }

        private IEnumerable<MTFVariable> GetAllVariables(Term term)
        {
            var output = new List<MTFVariable>();
            term.ForEachTerm<VariableTerm>(x => output.Add(x.MTFVariable));
            term.ForEachTerm<TermWrapper>(x => output.AddRange(ProcessVarianlesInObjects(x)));
            if (!hasTable)
            {
                term.ForEachTerm<ValidationTableTerm>(x => hasTable = true);
            }
            if (!hasTable)
            {
                term.ForEachTerm<ValidationTableResultTerm>(x => hasTable = true);
            }
            return output;
        }

        private IEnumerable<MTFVariable> ProcessVarianlesInObjects(TermWrapper termWrapper)
        {
            var output = new List<MTFVariable>();
            var gci = termWrapper.Value as GenericClassInstanceConfiguration;
            if (gci != null)
            {
                output.AddRange(ProcessGci(gci));
            }
            else
            {
                var collection = termWrapper.Value as ICollection;
                if (collection != null)
                {
                    output.AddRange(ProcessVarianlesInCollection(collection));
                }
            }
            return output;
        }

        private IEnumerable<MTFVariable> ProcessGci(GenericClassInstanceConfiguration gci)
        {
            var output = new List<MTFVariable>();
            if (gci.PropertyValues != null)
            {
                foreach (var item in gci.PropertyValues)
                {
                    if (item != null)
                    {
                        var term = item.Value as Term;
                        if (term != null)
                        {
                            output.AddRange(GetAllVariables(term));
                        }
                        else
                        {
                            var collection = item.Value as ICollection;
                            if (collection != null)
                            {
                                output.AddRange(ProcessVarianlesInCollection(collection));
                            }
                        }
                    }
                }
            }
            return output;
        }

        private IEnumerable<MTFVariable> ProcessVarianlesInCollection(ICollection collection)
        {
            var output = new List<MTFVariable>();
            foreach (var item in collection)
            {
                var term = item as Term;
                if (term != null)
                {
                    output.AddRange(GetAllVariables(term));
                }
                else
                {
                    var gci = item as GenericClassInstanceConfiguration;
                    if (gci!=null)
                    {
                        output.AddRange(ProcessGci(gci));
                    }
                }
            }
            return output;
        }


        public IEnumerable<MergeActivitiesData<MTFVariable>> CurrentVariables
        {
            get { return mergedData.VariablesMapping; }
        }

        public override string Title
        {
            get { return "Merge variables"; }
        }

        public override string Description
        {
            get { return "Select available variables or create new."; }
        }

        public bool HasTable
        {
            get { return hasTable; }
        }
    }
}
