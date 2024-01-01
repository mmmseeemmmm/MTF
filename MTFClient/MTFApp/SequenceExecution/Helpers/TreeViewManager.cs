using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MTFApp.SequenceExecution.ExtendedMode;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;

namespace MTFApp.SequenceExecution.Helpers
{
    public class TreeViewManager : NotifyPropertyBase
    {
        private readonly Dictionary<List<Guid>, int> indexes = new Dictionary<List<Guid>, int>();

        private readonly List<MTFActivityVisualisationWrapper> sourceActivities =
            new List<MTFActivityVisualisationWrapper>();

        private ObservableCollection<MTFActivityVisualisationWrapper> treeActivities =
            new ObservableCollection<MTFActivityVisualisationWrapper>();

        private bool existSetupMode;
        private readonly object treeLock = new object();
        private readonly ExecutionPointerHelper executionPointer = new ExecutionPointerHelper();
        private readonly DynamicSequenceHelper dynamicHelper = new DynamicSequenceHelper();
        private readonly ExtendedModeManager extendedModeManager;
        private bool allowDispatcherActions = true;

        public TreeViewManager(ExtendedModeActions extendedModeActions)
        {
            ActivityDetailChangeAtRuntime = true;
            extendedModeManager = new ExtendedModeManager(extendedModeActions);
        }

        #region Properties

        public ObservableCollection<MTFActivityVisualisationWrapper> TreeActivities => treeActivities;

        public IEnumerable<MTFActivityVisualisationWrapper> ParallelActivities { get; private set; }

        public bool IsEmpty => treeActivities.Count == 0 || indexes.Count == 0;

        public bool ExistSetupMode
        {
            get { return existSetupMode; }
        }

        public bool ActivityDetailChangeAtRuntime { get; set; }

        public ExecutionPointerHelper ExecutionPointer
        {
            get { return executionPointer; }
        }

        #endregion

        #region public methods

        public void Stop()
        {
            allowDispatcherActions = false;
        }

        public void Start()
        {
            allowDispatcherActions = true;
        }

        public void AddToSource(MTFActivityVisualisationWrapper activityWrapper)
        {
            if (activityWrapper.HasSetup)
            {
                if (!existSetupMode)
                {
                    existSetupMode = true;
                }

                extendedModeManager.AddSetupPoint(activityWrapper);
            }

            indexes.Add(activityWrapper.GuidPath, sourceActivities.Count);
            sourceActivities.Add(activityWrapper);
        }

        public void Clear()
        {
            sourceActivities.Clear();
            indexes.Clear();
            lock (treeLock)
            {
                treeActivities.Clear();
            }

            extendedModeManager.Clear();
            existSetupMode = false;
        }

        public void FillTree()
        {
            if (sourceActivities != null)
            {
                lock (treeLock)
                {
                    treeActivities = new ObservableCollection<MTFActivityVisualisationWrapper>(sourceActivities.Where(x => x.Nesting == 0));
                    NotifyPropertyChanged("TreeActivities");
                }
            }
        }

        public void ChangeTree(MTFActivityVisualisationWrapper executionItem, bool insert)
        {
            if (executionItem.IsExpandable)
            {
                int baseNesting = executionItem.Nesting;
                executionItem.IsCollapsed = !insert;

                if (!executionItem.IsDynamicActivity && !executionItem.IsDynamicRoot)
                {
                    var index = indexes[executionItem.GuidPath];
                    if (insert)
                    {
                        InsertIntoTreeActivities(index, baseNesting, executionItem, sourceActivities);
                    }
                    else
                    {
                        RemoveFromTree(index, baseNesting, executionItem, sourceActivities);
                    }
                }
                else
                {
                    var dynamicSource = dynamicHelper.GetDynamicActivities(executionItem);
                    if (dynamicSource != null)
                    {
                        if (insert)
                        {
                            InsertIntoTreeActivities(-1, baseNesting, executionItem, dynamicSource.ToList());
                        }
                        else
                        {
                            RemoveFromTree(-1, baseNesting, executionItem, dynamicSource.ToList());
                        }
                    }
                }
            }
        }

        public void LoadDynamic(MTFSequence dynamicSequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences)
        {
            dynamicHelper.LoadSequence(dynamicSequence, externalSequences);
        }

        public void InsertDynamicActivities(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath)
        {
            if (dynamicHelper.ContainsSequence(sequenceId))
            {
                var dynamicSequence = dynamicHelper.GetSequence(sequenceId);

                var dynamicCallAcitivityContainer = indexes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.Key, activityIdPath));
                if (dynamicCallAcitivityContainer.Key != null)
                {
                    var dynamicCallAcitivity = sourceActivities[dynamicCallAcitivityContainer.Value];
                    if (dynamicCallAcitivity != null)
                    {
                        dynamicHelper.CheckDynamicSequence(dynamicSequence.Id);

                        IList<MTFSequenceActivity> activityCollection = null;
                        if (callSubSequence)
                        {
                            if (dynamicSequence.ActivitiesByCall != null)
                            {
                                var dynamicSubSequence = dynamicSequence.ActivitiesByCall.FirstOrDefault(x => x.Id == subSequenceId);
                                if (dynamicSubSequence != null)
                                {
                                    activityCollection = new List<MTFSequenceActivity> {dynamicSubSequence};
                                }
                            }
                        }
                        else
                        {
                            activityCollection = dynamicSequence.MTFSequenceActivities;
                        }

                        if (activityCollection != null && dynamicHelper.CanInsert(dynamicCallAcitivity))
                        {
                            MTFSequenceHelper.TransformActivitiesToExecution(dynamicSequence,
                                activityCollection, x => AssignDynamicActivity(x, sequenceId, dynamicCallAcitivity),
                                true, dynamicCallAcitivity.Nesting + 1, activityIdPath.ToList(), new List<string>());


                            ChangeTree(dynamicCallAcitivity, true);
                            AssignDynamicToExtendedMode();
                        }
                    }
                }
            }
        }

        private void AssignDynamicActivity(MTFActivityVisualisationWrapper activityWrapper, Guid sequenceId,
            MTFActivityVisualisationWrapper dynamicCallAcitivity)
        {
            dynamicHelper.AssignDynamicActivity(activityWrapper, sequenceId, dynamicCallAcitivity);
            if (activityWrapper.HasSetup)
            {
                extendedModeManager.AddSetupPoint(activityWrapper);
            }
        }


        public void DynamicUnload(Guid sequenceId)
        {
            dynamicHelper.UnLoad(sequenceId, RemoveFromTree);
        }

        public List<Guid> GetDynamicVariablesBySequence(Guid sequenceId)
        {
            return dynamicHelper.GetDynamicVariablesBySequence(sequenceId);
        }

        public void RepeatSubSequence(Guid[] executingActivityPath)
        {
            var subSequenceToRepeat = FindInSourceWithIndexes(executingActivityPath);
            if (subSequenceToRepeat != null)
            {
                if (subSequenceToRepeat.IsDynamicActivity)
                {
                    var index = dynamicHelper.GetIndex(subSequenceToRepeat.GuidPath) + 1;
                    ClearResults(dynamicHelper.DynamicSource, index, subSequenceToRepeat.Nesting);
                }
                else
                {
                    var index = indexes[subSequenceToRepeat.GuidPath] + 1;
                    ClearResults(sourceActivities, index, subSequenceToRepeat.Nesting);
                }
            }
        }

        public void ClearResults(IList<MTFActivityVisualisationWrapper> collection, int startIndex, int nesting)
        {
            while (startIndex < collection.Count && collection[startIndex].Nesting > nesting)
            {
                var activity = collection[startIndex];
                if (activity.IsDynamicRoot)
                {
                    dynamicHelper.ClearResults(activity);
                }

                activity.Result = null;
                activity.Status = MTFExecutionActivityStatus.None;
                startIndex++;
            }
        }

        public void ShowActivityInTree(MTFActivityVisualisationWrapper activity)
        {
            if (!activity.IsInTree && !activity.IsDynamicActivity)
            {
                var index = indexes[activity.GuidPath];
                var rootActivity = sourceActivities[index];
                var currentNesting = activity.Nesting;
                var subsequences = new List<MTFActivityVisualisationWrapper>();

                while (index > 0 && rootActivity.Nesting != 0)
                {
                    index--;
                    rootActivity = sourceActivities[index];
                    if (rootActivity.Nesting < currentNesting)
                    {
                        subsequences.Add(sourceActivities[index]);
                        currentNesting = rootActivity.Nesting;
                    }
                }

                for (int i = subsequences.Count - 1; i >= 0; i--)
                {
                    ChangeTree(subsequences[i], true);
                }
            }
        }

        public void ClearResults()
        {
            foreach (var activity in sourceActivities)
            {
                activity.Result = null;
                activity.Status = MTFExecutionActivityStatus.None;
                dynamicHelper.Clear(RemoveFromTree);
            }
        }

        public MTFActivityVisualisationWrapper FindInSourceWithIndexes(IList<Guid> guidArray)
        {
            var index = indexes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.Key, guidArray));
            if (index.Key != null)
            {
                return sourceActivities[index.Value];
            }

            return dynamicHelper.FindInDynamicWithIndexes(guidArray);
        }

        public void GenerateParallelActivities(MTFActivityVisualisationWrapper activity)
        {
            ParallelActivities = CreateParallelCollection(activity);
            NotifyPropertyChanged("ParallelActivities");
        }

        public void AssignNewActivityResultInRuntime(MTFActivityResult result)
        {
            var item = FindInSourceWithIndexes(result.ActivityIdPath);
            if (item != null)
            {
                item.Status = result.Status;
                item.Result = result;
                result.Parent = item.Activity;

                if (item.Activity is MTFCallActivityBase && ActivityDetailChangeAtRuntime)
                {
                    ChangeTree(item, false);
                }
            }
        }

        public void InitExtendedMode(string sequenceName, Guid sequenceId)
        {
            extendedModeManager.Init(sequenceName);

            var debugData = extendedModeManager.BreakPointData;
            if (debugData.SequenceId == sequenceId && debugData.ListActivities != null)
            {
                foreach (var activitySetting in debugData.ListActivities)
                {
                    if (!activitySetting.IsDynamic)
                    {
                        var activity = FindInSourceWithoutDynamic(activitySetting.GuidPath);
                        if (activity != null)
                        {
                            activity.IsBreakPoint = activitySetting.State;
                            activitySetting.AssignActivity(activity);
                        }
                    }

                    extendedModeManager.AddBreakPoint(activitySetting);
                }
            }
        }

        private void AssignDynamicToExtendedMode()
        {
            var debugData = extendedModeManager.BreakPoints.Where(x => !x.IsLoaded);
            foreach (var activitySetting in debugData)
            {
                var activity = dynamicHelper.FindInDynamicWithIndexes(activitySetting.GuidPath);
                if (activity != null)
                {
                    activity.IsBreakPoint = activitySetting.State;
                    activitySetting.AssignActivity(activity);
                }
            }
        }

        public IEnumerable<List<Guid>> GetActiveBreakPoints()
        {
            return extendedModeManager.BreakPoints.Where(x => x.State == StateDebugSetup.Active).Select(x => x.GuidPath);
        }

        public IEnumerable<List<Guid>> GetActiveSetupModes()
        {
            return extendedModeManager.SetupModes.Where(x => x.State == StateDebugSetup.Active).Select(x => x.GuidPath);
        }

        public bool EnableDebug
        {
            get { return extendedModeManager.EnableDebug; }
        }

        public bool EnableSetup
        {
            get { return extendedModeManager.EnableSetup; }
        }

        public void SaveExtendedModeData(string sequenceName, Guid sequenceId)
        {
            extendedModeManager.SaveData(sequenceId, sequenceName, ExtendedMode.ExtendedModeTypes.Debug);
            extendedModeManager.SaveData(sequenceId, sequenceName, ExtendedMode.ExtendedModeTypes.Setup);
        }

        public void SaveExtendedModeData(string sequenceName, Guid sequenceId, ExtendedMode.ExtendedModeTypes mode)
        {
            extendedModeManager.SaveData(sequenceId, sequenceName, mode);
        }

        public void SwitchExtendedMode(bool state, ExtendedMode.ExtendedModeTypes mode)
        {
            extendedModeManager.SwichtExtendedMode(state, mode);
        }

        public void ChangePointState(MTFActivityVisualisationWrapper activityWrapper, ExtendedMode.ExtendedModeTypes mode)
        {
            extendedModeManager.ChangePointState(activityWrapper, mode);
        }

        public void ExpandToSourceErrorItem(MTFActivityVisualisationWrapper activityWrapper)
        {
            if (activityWrapper.IsCollapsed)
            {
                lock (treeLock)
                {
                    allowDispatcherActions = true;
                    ExpandToSourceError(activityWrapper);
                }
            }
            else
            {
                ChangeTree(activityWrapper, false);
            }
        }


        #endregion

        #region private methods

        private MTFActivityVisualisationWrapper FindInSourceWithoutDynamic(IList<Guid> guidArray)
        {
            var index = indexes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.Key, guidArray));
            return index.Key != null ? sourceActivities[index.Value] : null;
        }

        private IEnumerable<MTFActivityVisualisationWrapper> CreateParallelCollection(MTFActivityVisualisationWrapper activity)
        {
            var nestingToAdd = activity.Nesting + 1;
            IList<MTFActivityVisualisationWrapper> collection;
            int startIndex;

            if (activity.IsDynamicActivity)
            {
                startIndex = dynamicHelper.GetIndex(activity.GuidPath) + 1;
                collection = dynamicHelper.DynamicSource;
            }
            else
            {
                startIndex = indexes[activity.GuidPath] + 1;
                collection = sourceActivities;
            }

            return GetParallelCollection(collection, startIndex, nestingToAdd);
        }

        private IEnumerable<MTFActivityVisualisationWrapper> GetParallelCollection(IList<MTFActivityVisualisationWrapper> collection,
            int startIndex, int nestingToAdd)
        {
            for (int i = startIndex; i < collection.Count; i++)
            {
                var nextActivity = collection[i];
                if (nextActivity.Nesting < nestingToAdd)
                {
                    break;
                }

                if (nextActivity.Nesting == nestingToAdd)
                {
                    yield return nextActivity;
                }
            }
        }

        private void InvokeOnAsyncDispatcher(Action a)
        {
            Application.Current.Dispatcher.BeginInvoke(a, DispatcherPriority.ContextIdle);
        }

        private void RemoveFromTree(int index, int baseNesting, MTFActivityVisualisationWrapper executionItem,
            List<MTFActivityVisualisationWrapper> source)
        {
            var indexOfSubSequence = index + 1;
            var deletedItems = new List<MTFActivityVisualisationWrapper>();
            lock (treeLock)
            {
                while (indexOfSubSequence < source.Count && source[indexOfSubSequence].Nesting > baseNesting)
                {
                    var activity = source[indexOfSubSequence];
                    activity.CanInsert = false;
                    deletedItems.Add(activity);
                    indexOfSubSequence++;
                }
            }

            InvokeOnAsyncDispatcher(() => Remove(executionItem, deletedItems));
        }

        private void InsertIntoTreeActivities(int startIndex, int baseNesting,
            MTFActivityVisualisationWrapper executionItem, List<MTFActivityVisualisationWrapper> source)
        {
            int nestingToChangeVisibility = baseNesting + 1;

            var insertedItems = new List<MTFActivityVisualisationWrapper>();

            lock (treeLock)
            {
                for (int i = startIndex + 1; i < source.Count; i++)
                {
                    var activityToInsert = source[i];
                    activityToInsert.CanInsert = true;
                    if (activityToInsert.Nesting <= baseNesting)
                    {
                        break;
                    }

                    if (activityToInsert.Nesting <= nestingToChangeVisibility)
                    {
                        if (activityToInsert.Nesting < nestingToChangeVisibility)
                        {
                            nestingToChangeVisibility = activityToInsert.Nesting;
                        }

                        insertedItems.Add(activityToInsert);

                        if (activityToInsert.IsExpandable && !activityToInsert.IsCollapsed)
                        {
                            nestingToChangeVisibility++;
                        }
                    }
                }
            }

            InvokeOnAsyncDispatcher(() => Insert(executionItem, insertedItems));
        }

        private void Insert(MTFActivityVisualisationWrapper executionItem, List<MTFActivityVisualisationWrapper> insertedItems)
        {
            if (!allowDispatcherActions)
            {
                return;
            }

            lock (treeLock)
            {
                var parentIndex = treeActivities.IndexOf(executionItem);

                if (parentIndex != -1)
                {
                    for (int i = 0; i < insertedItems.Count; i++)
                    {
                        var activity = insertedItems[i];
                        if (!activity.IsInTree && activity.CanInsert)
                        {
                            var newIndex = parentIndex + 1 + i;
                            if (newIndex > 0 && newIndex <= treeActivities.Count)
                            {
                                treeActivities.Insert(newIndex, activity);
                                activity.IsInTree = true;
                            }
                        }
                    }
                }
            }
        }

        private void Remove(MTFActivityVisualisationWrapper executionItem, List<MTFActivityVisualisationWrapper> removedItems)
        {
            if (!allowDispatcherActions)
            {
                return;
            }

            lock (treeLock)
            {
                var indexOfSubsequence = treeActivities.IndexOf(executionItem);
                foreach (var activity in removedItems)
                {
                    if (activity.IsInTree)
                    {
                        var newIndex = indexOfSubsequence + 1;
                        if (newIndex > 0 && newIndex < treeActivities.Count)
                        {
                            treeActivities.RemoveAt(newIndex);
                            activity.IsInTree = false;
                        }
                    }
                }
            }
        }

        private async void ExpandToSourceError(MTFActivityVisualisationWrapper activityWrapper)
        {
            ChangeTree(activityWrapper, true);
            List<MTFActivityVisualisationWrapper> errorActivities = null;
            var currentSubSequence = activityWrapper;

            while (currentSubSequence != null)
            {
                var errorItem = currentSubSequence;
                var waitAction = new Action(() => errorActivities = GetErrorActivities(errorItem));
                await Application.Current.Dispatcher.BeginInvoke(waitAction, DispatcherPriority.ContextIdle);

                if (errorActivities.Count == 1)
                {
                    currentSubSequence = errorActivities[0];
                    ChangeTree(currentSubSequence, true);
                }
                else
                {
                    currentSubSequence = null;
                }
            }


        }

        private List<MTFActivityVisualisationWrapper> GetErrorActivities(MTFActivityVisualisationWrapper activityWrapper)
        {
            var errorActivities = new List<MTFActivityVisualisationWrapper>();

            var index = treeActivities.IndexOf(activityWrapper);

            int actualNesting = activityWrapper.Nesting + 1;
            int nextNesting = actualNesting;

            if (index != -1)
            {
                index++;
                while (index < treeActivities.Count && nextNesting == actualNesting)
                {
                    var item = treeActivities[index];
                    nextNesting = item.Nesting;
                    index++;
                    if (item.Status == MTFExecutionActivityStatus.Nok && nextNesting == actualNesting)
                    {
                        errorActivities.Add(item);
                    }
                }
            }
            return errorActivities;
        }


        #endregion

        
    }
}