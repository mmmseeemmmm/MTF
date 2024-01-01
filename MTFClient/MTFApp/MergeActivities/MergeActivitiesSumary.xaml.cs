using MTFClientServerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.UIHelpers;
using MTFClientServerCommon.Mathematics;
using UniqueNameHeleper = MTFApp.UIHelpers.UniqueNameHeleper;

namespace MTFApp.MergeActivities
{
    /// <summary>
    /// Interaction logic for MergeSumary.xaml
    /// </summary>
    public partial class MergeActivitiesSumary : MergeActivitiesBase
    {
        private readonly MTFSequence sequence;
        private readonly MergeSharedData mergedData;
        private readonly List<MTFSequenceClassInfo> componentsToAdd = new List<MTFSequenceClassInfo>();
        private readonly List<MTFVariable> variablesToAdd = new List<MTFVariable>();
        private string mergeProgress;

        public MergeActivitiesSumary(MTFSequence sequence, MergeSharedData mergedData)
        {
            InitializeComponent();
            this.sequence = sequence;
            this.mergedData = mergedData;
            this.Root.DataContext = this;
        }

        public override string Title
        {
            get { return "Merge sumary"; }
        }

        public override string Description
        {
            get { return "Merge sumary"; }
        }

        public override bool IsLastControl
        {
            get { return true; }
        }

        public IEnumerable<MergeActivitiesData<MTFSequenceClassInfo>> Components
        {
            get { return mergedData.ComponentsMapping; }
        }

        public IEnumerable<MergeActivitiesData<MTFVariable>> Variables
        {
            get { return mergedData.VariablesMapping; }
        }

        public string MergeProgress
        {
            get { return mergeProgress; }
            set
            {
                mergeProgress = value;
                NotifyPropertyChanged();
            }
        }

        protected override void OnShow()
        {
            NotifyPropertyChanged("Components");
            NotifyPropertyChanged("Variables");
            RunPrecessAsync();
            base.OnShow();
        }

        protected override bool OnFinis()
        {
            foreach (var mtfSequenceClassInfo in componentsToAdd)
            {
                sequence.MTFSequenceClassInfos.Add(mtfSequenceClassInfo);
            }
            foreach (var mtfVariable in variablesToAdd)
            {
                sequence.MTFVariables.AddSorted(mtfVariable);
            }
            return base.OnFinis();
        }

        private async void RunPrecessAsync()
        {
            AllowFinnish = false;
            MergeProgress = "Running";
            await Task.Run(() => CreateSequenceClassInfos());
            await Task.Run(() => CreateVariables());
            await Task.Run(() => Merge());
            MergeProgress = "Finnish";
            AllowFinnish = true;
        }

        private void CreateVariables()
        {
            foreach (var activitiesVariable in mergedData.VariablesMapping)
            {
                var newVariable = activitiesVariable.NewData as NewVariable;
                if (newVariable != null)
                {
                    var type = Type.GetType(activitiesVariable.OriginalData.TypeName);
                    if (type != null)
                    {
                        var variable = activitiesVariable.OriginalData.Clone() as MTFVariable;
                        UniqueNameHeleper.AdjustName(variable, sequence.MTFVariables);
                        variablesToAdd.Add(variable);
                        activitiesVariable.NewData = variable;
                    }
                }
            }
        }

        private void CreateSequenceClassInfos()
        {
            foreach (var activitiesData in mergedData.ComponentsMapping)
            {
                var newData = activitiesData.NewData as NewSequenceClassInfo;
                if (newData != null)
                {
                    var newSequenceClassInfo = new MTFSequenceClassInfo
                    {
                        MTFClass = (MTFClassInfo)activitiesData.OriginalData.MTFClass.Copy(),
                        Alias = activitiesData.OriginalData.MTFClass.Name,
                        MTFClassInstanceConfiguration = activitiesData.OriginalData.MTFClassInstanceConfiguration
                    };
                    UniqueNameHeleper.AdjustName(newSequenceClassInfo, sequence.MTFSequenceClassInfos);
                    componentsToAdd.Add(newSequenceClassInfo);
                    activitiesData.NewData = newSequenceClassInfo;
                }
            }
        }

        private void Merge()
        {
            foreach (var activity in mergedData.AllActivities)
            {
                if (activity.ClassInfo != null)
                {
                    var newClassInfo = mergedData.ComponentsMapping.FirstOrDefault(x => x.OriginalData == activity.ClassInfo);
                    if (newClassInfo != null)
                    {
                        activity.ClassInfo = newClassInfo.NewData;
                    }
                }

                var setVariable = activity as MTFVariableActivity;
                if (setVariable != null)
                {
                    ChangeVariables(setVariable);
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
                                ChangeVariables(termValue);
                            }
                        }
                    }
                    if (activity.Term != null)
                    {
                        ChangeVariables(activity.Term);
                    }
                }

            }
        }

        private void ChangeVariables(MTFVariableActivity variableActivity)
        {
            if (variableActivity.Variable != null)
            {
                var mergedVariable = mergedData.VariablesMapping.FirstOrDefault(x => x.OriginalData == variableActivity.Variable);
                if (mergedVariable != null)
                {
                    variableActivity.Variable = mergedVariable.NewData;
                }
            }
            if (variableActivity.Value != null)
            {
                ChangeVariables(variableActivity.Value);
            }
        }

        private void ChangeVariables(Term term)
        {
            term.ForEachTerm<VariableTerm>(x =>
                                           {
                                               var mergedVariable =
                                                   mergedData.VariablesMapping.FirstOrDefault(v => v.OriginalData == x.MTFVariable);
                                               if (mergedVariable != null)
                                               {
                                                   x.MTFVariable = mergedVariable.NewData;
                                               }
                                           });

            term.ForEachTerm<ActivityResultTerm>(x =>
                                                 {
                                                     if (!mergedData.AllActivities.Contains(x.Value))
                                                     {
                                                         x.Value = null;
                                                     }
                                                 });

            term.ForEachTerm<TermWrapper>(ChangeVariablesInObjects);
        }

        private void ChangeVariablesInObjects(TermWrapper termWrapper)
        {
            var gci = termWrapper.Value as GenericClassInstanceConfiguration;
            if (gci!=null)
            {
                ProcessGci(gci);
            }
        }

        private void ProcessGci(GenericClassInstanceConfiguration gci)
        {
            if (gci.PropertyValues != null)
            {
                foreach (var item in gci.PropertyValues)
                {
                    if (item != null)
                    {
                        var term = item.Value as Term;
                        if (term != null)
                        {
                            ChangeVariables(term);
                        }
                        else
                        {
                            var collection = item.Value as ICollection;
                            if (collection != null)
                            {
                                ProcessVarianlesInCollection(collection);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessVarianlesInCollection(ICollection collection)
        {
            foreach (var item in collection)
            {
                var term = item as Term;
                if (term != null)
                {
                    ChangeVariables(term);
                }
                else
                {
                    var gci = item as GenericClassInstanceConfiguration;
                    if (gci != null)
                    {
                        ProcessGci(gci);
                    }
                }
            }
        }
    }
}
