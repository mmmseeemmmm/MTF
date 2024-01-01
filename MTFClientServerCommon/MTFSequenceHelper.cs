using MTFClientServerCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTFClientServerCommon.ClassInformation;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon
{
    public static class MTFSequenceHelper
    {
        public static MTFSequenceActivity GetActivity(this MTFSequence sequence, Guid activityId)
        {
            var activity = GetActivity(sequence.MTFSequenceActivities, activityId) ?? GetActivity(sequence.ActivitiesByCall, activityId);
            if (activity == null && sequence.ExternalSubSequences != null)
            {
                foreach (var externalInfo in sequence.ExternalSubSequences)
                {
                    activity = GetActivity(externalInfo.ExternalSequence, activityId);
                    if (activity != null)
                    {
                        return activity;
                    }
                }
            }

            return activity;
        }

        public static bool Any(this MTFSequence sequence, Func<MTFSequenceActivity, bool> func)
        {
            return sequence.MTFSequenceActivities.MTFAny(func) || sequence.ActivitiesByCall.MTFAny(func);
        }

        private static bool MTFAny(this IEnumerable<MTFSequenceActivity> activities, Func<MTFSequenceActivity, bool> func)
        {
            foreach (var activity in activities)
            {
                if (func(activity))
                {
                    return true;
                }

                if (activity is MTFSubSequenceActivity subSequence)
                {
                    if (subSequence.ExecutionType == ExecutionType.ExecuteByCase)
                    {
                        if (subSequence.Cases != null && subSequence.Cases.Count > 0)
                        {
                            foreach (var mtfCase in subSequence.Cases)
                            {
                                var output = mtfCase.Activities.MTFAny(func);
                                if (output)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        var output = subSequence.Activities.MTFAny(func);
                        if (output)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void ForEachActivity(this MTFSequence sequence, Action<MTFSequenceActivity> func)
        {
            ForEachActivity(sequence.MTFSequenceActivities, func);
            ForEachActivity(sequence.ActivitiesByCall, func);
        }

        public static void ForEachActivity<T>(this MTFSequence sequence, Action<T> action) where T : MTFSequenceActivity
        {
            ForEachActivity(sequence.MTFSequenceActivities, action);
            ForEachActivity(sequence.ActivitiesByCall, action);
        }

        public static void ForEachActivity(this IEnumerable<MTFSequenceActivity> activities, Action<MTFSequenceActivity> func)
        {
            foreach (var activity in activities)
            {
                func(activity);

                if (activity is MTFSubSequenceActivity subSequenceActivity)
                {
                    if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                    {
                        if (subSequenceActivity.Cases != null && subSequenceActivity.Cases.Count > 0)
                        {
                            foreach (var mtfCase in subSequenceActivity.Cases)
                            {
                                ForEachActivity(mtfCase.Activities, func);
                            }
                        }
                    }
                    else
                    {
                        ForEachActivity(subSequenceActivity.Activities, func);
                    }
                }
            }
        }

        private static void ForEachActivity<T>(this IEnumerable<MTFSequenceActivity> activities, Action<T> action) where T : MTFSequenceActivity
        {
            if (activities != null)
            {
                foreach (var activity in activities)
                {
                    if (activity is T foundActivity)
                    {
                        action(foundActivity);
                    }

                    if (activity is MTFSubSequenceActivity subSequenceActivity)
                    {
                        if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                        {
                            if (subSequenceActivity.Cases != null && subSequenceActivity.Cases.Count > 0)
                            {
                                foreach (var mtfCase in subSequenceActivity.Cases)
                                {
                                    ForEachActivity(mtfCase.Activities, action);
                                }
                            }
                        }
                        else
                        {
                            ForEachActivity(subSequenceActivity.Activities, action);
                        }
                    }
                }
            }
        }

        public static IEnumerable<MTFSequenceActivity> GetAllActivitiesAsPlainCollection(IEnumerable<MTFSequenceActivity> activities)
        {
            var collection = new List<MTFSequenceActivity>();
            if (activities != null)
            {
                foreach (var mtfSequenceActivity in activities)
                {
                    collection.Add(mtfSequenceActivity);

                    if (mtfSequenceActivity is MTFSubSequenceActivity subSequence)
                    {
                        if (subSequence.Cases != null)
                        {
                            foreach (var mtfCase in subSequence.Cases)
                            {
                                collection.AddRange(GetAllActivitiesAsPlainCollection(mtfCase.Activities));
                            }
                        }
                        collection.AddRange(GetAllActivitiesAsPlainCollection(subSequence.Activities));
                    }
                }
            }
            return collection;
        }

        public static void TransformActivitiesToExecution(MTFSequence sequence, IEnumerable<MTFSequenceActivity> activities,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            foreach (var activity in activities)
            {
                TransformActivityToExecution(sequence, activity, func, onlyActive, nesting, guidPath, missingReferences);
            }
        }

        private static MTFActivityVisualisationWrapper TransformActivityToExecution(MTFSequence sequence, MTFSequenceActivity activity,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            if (activity == null)
            {
                return null;
            }

            if (guidPath.Contains(activity.Id))
            {
                throw new Exception($"Recursion has been detected in the activity: {activity.ActivityName}");
            }

            guidPath.Add(activity.Id);
            int guidIndex = guidPath.Count - 1;
            MTFActivityVisualisationWrapper wrapper = null;
            if (!onlyActive || activity.IsActive)
            {
                var sequenceHandlingActivity = activity as MTFSequenceHandlingActivity;
                if (sequenceHandlingActivity != null &&
                    sequenceHandlingActivity.SequenceHandlingType == SequenceHandlingType.ChangeCommandsStatus)
                {
                    guidPath.RemoveRange(guidIndex, guidPath.Count - guidIndex);
                    return null;
                }

                var executeActivity = activity as MTFExecuteActivity;
                if (executeActivity != null)
                {
                    wrapper = TransformExecuteActivity(sequence, executeActivity, func, onlyActive, nesting, guidPath, missingReferences);
                }
                else
                {
                    var subSequenceActivity = activity as MTFSubSequenceActivity;
                    if (subSequenceActivity != null)
                    {
                        wrapper = TransformSubSequenceActivity(sequence, subSequenceActivity, func, onlyActive, nesting, guidPath,
                            missingReferences);
                    }
                    else
                    {
                        wrapper = CreateWrapper(activity, nesting, guidPath);
                        func(wrapper);
                    }
                }
            }
            guidPath.RemoveRange(guidIndex, guidPath.Count - guidIndex);
            return wrapper;
        }

        private static MTFActivityVisualisationWrapper TransformSubSequenceActivity(MTFSequence sequence,
            MTFSubSequenceActivity subSequenceActivity,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            var subSequenceWrapper = CreateWrapper(subSequenceActivity, nesting, guidPath);
            func(subSequenceWrapper);

            if (!subSequenceActivity.IsExecuteAsOneActivity)
            {
                if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                {
                    if (subSequenceActivity.Cases != null)
                    {
                        foreach (var mtfCase in subSequenceActivity.Cases)
                        {
                            guidPath.Add(mtfCase.Id);
                            var caseWrapper =
                                CreateWrapper(new MTFSubSequenceActivity { Id = mtfCase.Id, ActivityName = mtfCase.Name, IsCollapsed = true },
                                    nesting + 1, guidPath);

                            func(caseWrapper);

                            foreach (var caseActivity in mtfCase.Activities)
                            {
                                var activityWrapper = TransformActivityToExecution(sequence, caseActivity, func, onlyActive, nesting + 2,
                                    guidPath, missingReferences);
                                if (!caseWrapper.HasSetup && activityWrapper != null)
                                {
                                    caseWrapper.HasSetup = activityWrapper.HasSetup;
                                }
                            }

                            guidPath.RemoveAt(guidPath.Count - 1);
                            if (!subSequenceWrapper.HasSetup)
                            {
                                subSequenceWrapper.HasSetup = caseWrapper.HasSetup;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var activity in subSequenceActivity.Activities)
                    {
                        var activityWrapper = TransformActivityToExecution(sequence, activity, func, onlyActive, nesting + 1, guidPath,
                            missingReferences);
                        if (!subSequenceWrapper.HasSetup && activityWrapper != null)
                        {
                            subSequenceWrapper.HasSetup = activityWrapper.HasSetup;
                        }
                    }
                }
            }
            return subSequenceWrapper;
        }

        private static MTFActivityVisualisationWrapper TransformExecuteActivity(MTFSequence sequence, MTFExecuteActivity executeActivity,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            var wrapper = CreateWrapper(executeActivity, nesting, guidPath);
            func(wrapper);
            MTFActivityVisualisationWrapper innerWrapper = null;
            switch (executeActivity.Type)
            {
                case ExecuteActyvityTypes.Local:
                    innerWrapper = TrasformLocalExecuteActivity(sequence, executeActivity, func, onlyActive, nesting, guidPath, missingReferences);
                    break;
                case ExecuteActyvityTypes.External:
                    innerWrapper = TransformExternalExecuteActivity(sequence, executeActivity, func, onlyActive, nesting, guidPath,
                        missingReferences);
                    break;
                case ExecuteActyvityTypes.Dynamic:
                    wrapper.IsDynamicRoot = true;
                    break;
            }
            if (innerWrapper != null)
            {
                wrapper.HasSetup = innerWrapper.HasSetup;
            }
            return wrapper;
        }

        private static MTFActivityVisualisationWrapper TransformExternalExecuteActivity(MTFSequence sequence, MTFExecuteActivity externalActivity,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            MTFActivityVisualisationWrapper temporaryWrapper = new MTFActivityVisualisationWrapper();

            if (externalActivity.ExternalCall != null && !string.IsNullOrEmpty(externalActivity.ExternalCall.ExternalSequenceToCall))
            {
                var externalInfo = GetExternalSequenceByName(sequence, externalActivity.ExternalCall.ExternalSequenceToCall);
                if (externalInfo != null)
                {
                    if (externalInfo.IsEnabled)
                    {
                        var externalSequence = externalInfo.ExternalSequence;

                        if (externalActivity.ExternalCall != null)
                        {
                            if (externalActivity.ExternalCall.InnerSubSequenceByCallId != Guid.Empty &&
                                                externalActivity.ExternalCall.InnerSubSequenceByCallId != ActivityNameConstants.CallWholeSequenceId)
                            {
                                //load subsequence
                                bool subSequenceByCallIsFound = false;
                                if (externalSequence.ActivitiesByCall != null)
                                {
                                    var subSequenceByCall =
                                        externalSequence.ActivitiesByCall.FirstOrDefault(
                                                x => x.Id == externalActivity.ExternalCall.InnerSubSequenceByCallId);
                                    if (subSequenceByCall != null)
                                    {
                                        subSequenceByCallIsFound = true;
                                        var externalSubSequenceWrapper = TransformActivityToExecution(externalSequence, subSequenceByCall, func,
                                            onlyActive, nesting + 1, guidPath, missingReferences);
                                        if (externalSubSequenceWrapper != null && !temporaryWrapper.HasSetup)
                                        {
                                            temporaryWrapper.HasSetup = externalSubSequenceWrapper.HasSetup;
                                        }
                                    }
                                }

                                if (!subSequenceByCallIsFound)
                                {
                                    missingReferences.Add(GetMissingExternalSubSequenceError(sequence.Name, externalActivity));
                                }
                            }
                            else
                            {
                                //load whole sequence
                                foreach (var activity in externalSequence.MTFSequenceActivities)
                                {
                                    var activityWrapper = TransformActivityToExecution(externalSequence, activity, func, onlyActive, nesting + 1,
                                        guidPath, missingReferences);
                                    if (!temporaryWrapper.HasSetup && activityWrapper != null)
                                    {
                                        temporaryWrapper.HasSetup = activityWrapper.HasSetup;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    missingReferences.Add(GetMissingExternalSequenceError(sequence.Name, externalActivity));
                }
            }

            return temporaryWrapper;
        }

        private static MTFActivityVisualisationWrapper TrasformLocalExecuteActivity(MTFSequence sequence, MTFExecuteActivity executeActivity,
            Action<MTFActivityVisualisationWrapper> func, bool onlyActive, int nesting, List<Guid> guidPath, List<string> missingReferences)
        {
            var callActivity = executeActivity.ActivityToCall;
            MTFActivityVisualisationWrapper callWrapper = null;
            if (callActivity != null)
            {
                callWrapper = TransformActivityToExecution(sequence, callActivity, func, onlyActive, nesting + 1, guidPath, missingReferences);
            }
            else
            {
                missingReferences.Add(GetMissingExecuteActivityError(sequence.Name, executeActivity));
            }
            return callWrapper;
        }

        private static string GetMissingExternalSequenceError(string sequenceName, MTFExecuteActivity externalActivity)
        {
            return new StringBuilder().
                Append("Error in ExternalActivity:").
                Append(Environment.NewLine).
                Append(sequenceName).
                Append(" | ").
                Append(externalActivity.GetActivityPath()).
                Append(externalActivity.ActivityName).
                Append(" - external sequence (").
                Append(externalActivity.ExternalCall != null ? externalActivity.ExternalCall.ExternalSequenceToCall : string.Empty).
                Append(") was not found.").
                Append(Environment.NewLine).
                ToString();
        }

        private static string GetMissingExternalSubSequenceError(string sequenceName, MTFExecuteActivity externalActivity)
        {
            return new StringBuilder().
                Append("Error in ExternalActivity:").
                Append(Environment.NewLine).
                Append(sequenceName).
                Append(" | ").
                Append(externalActivity.GetActivityPath()).
                Append(externalActivity.ActivityName).
                Append(" - SubSequence ").
                Append(externalActivity.ExternalCall != null ? externalActivity.ExternalCall.OriginalCallActivityName : string.Empty).
                Append(" in external sequence").
                Append(externalActivity.ExternalCall != null ? externalActivity.ExternalCall.ExternalSequenceToCall : string.Empty).
                Append("  was not found.").
                Append(Environment.NewLine).
                ToString();
        }

        private static string GetMissingExecuteActivityError(string sequenceName, MTFSequenceActivity activity)
        {
            return new StringBuilder().
                Append("Error in ExecuteActivity:").
                Append(Environment.NewLine).
                Append(sequenceName).
                Append(" | ").
                Append(activity.GetActivityPath()).
                Append(activity.ActivityName).
                Append(": missing reference to SubSequence.").
                Append(Environment.NewLine).
                ToString();
        }


        private static MTFExternalSequenceInfo GetExternalSequenceByName(MTFSequence sequence, string subSequenceName)
        {
            return sequence.ExternalSubSequences != null
                ? sequence.ExternalSubSequences.FirstOrDefault(x => x != null && x.ExternalSequence.Name == subSequenceName)
                : null;
        }


        private static MTFActivityVisualisationWrapper CreateWrapper(MTFSequenceActivity activity, int nesting, List<Guid> guidPath)
        {
            return new MTFActivityVisualisationWrapper
            {
                Activity = activity,
                Nesting = nesting,
                GuidPath = guidPath.ToList(),
                IsInTree = nesting == 0,
                HasSetup = activity.SetupModeSupport,
                IsCollapsed = true,
                IsExpandable = IsExpandableActivity(activity),
                IsExecuteAsOneActivity = IsExecuteAsOne(activity)
            };
        }

        private static bool IsExecuteAsOne(MTFSequenceActivity activity)
        {
            var subSequenceActivity = activity as MTFSubSequenceActivity;
            return subSequenceActivity != null && subSequenceActivity.IsExecuteAsOneActivity;
        }

        private static bool IsExpandableActivity(MTFSequenceActivity activity)
        {
            var subSequenceActivity = activity as MTFSubSequenceActivity;
            if (subSequenceActivity != null)
            {
                return !subSequenceActivity.IsExecuteAsOneActivity;
            }
            var executeActivity = activity as MTFExecuteActivity;
            return executeActivity != null && (executeActivity.Type != ExecuteActyvityTypes.Dynamic ||
                                               executeActivity.DynamicActivityType == DynamicActivityTypes.Execute);
        }

        private static MTFSequenceActivity GetActivity(IEnumerable<MTFSequenceActivity> activities, Guid activityId)
        {
            foreach (var activity in activities)
            {
                if (activity is MTFSubSequenceActivity)
                {
                    MTFSubSequenceActivity subActivity = activity as MTFSubSequenceActivity;
                    var act = GetActivity(subActivity.Activities, activityId);
                    if (act != null)
                    {
                        return act;
                    }
                    if (subActivity.Cases != null && subActivity.Cases.Count > 0)
                    {
                        foreach (var c in subActivity.Cases)
                        {
                            act = GetActivity(c.Activities, activityId);
                            if (act != null)
                            {
                                return act;
                            }
                        }
                    }
                }

                if (activity != null && activity.Id == activityId)
                {
                    return activity;
                }
            }

            return null;
        }

        public static T GetActivityParentOfType<T>(this MTFSequenceActivity activity) where T : MTFDataTransferObject
        {
            MTFDataTransferObject parent = activity;
            do
            {
                parent = parent.Parent;
            }
            while (!(parent is T) && parent != null);
            return parent as T;
        }

        public static object RoundActualValue(object actualValue, IList<RoundingRule> roundingRules)
        {
            if (actualValue == null)
            {
                return null;
            }
            if (roundingRules?.Count > 0 && (actualValue is double || actualValue is float || actualValue is decimal))
            {
                double d;
                if (double.TryParse(actualValue.ToString(), out d))
                {
                    var absValue = Math.Abs(d);
                    var rule = roundingRules.FirstOrDefault(r => absValue >= r.Min && absValue <= r.Max);
                    if (rule != null)
                    {
                        var formater = new StringBuilder("0.").Append(new string('0', rule.Digits));
                        return Math.Round(d, rule.Digits).ToString(formater.ToString());
                    }
                }
            }
            return actualValue;
        }

        public static double RoundValue(double value, IList<RoundingRule> roundingRules)
        {
            if (roundingRules != null && roundingRules.Count > 0)
            {
                var absValue = Math.Abs(value);
                var rule = roundingRules.FirstOrDefault(r => absValue >= r.Min && absValue <= r.Max);
                if (rule != null)
                {
                    return Math.Round(value, rule.Digits);
                }
            }
            return value;
        }

        public static IEnumerable<ClientContolInfo> GetAvailableClientControls(this MTFSequence sequence)
        {
            if (sequence != null && sequence.MTFSequenceClassInfos != null && sequence.MTFSequenceClassInfos.Count > 0)
            {
                var clientControls = new List<ClientContolInfo>();
                foreach (var sequenceClassInfo in sequence.MTFSequenceClassInfos)
                {
                    if (sequenceClassInfo.MTFClass != null && sequenceClassInfo.MTFClass.ClientControlInfos != null)
                    {
                        foreach (var clientControl in sequenceClassInfo.MTFClass.ClientControlInfos)
                        {
                            if (clientControl.Type == ClientControlType.ClientUI &&
                                clientControls.All(c => c.AssemblyName != clientControl.AssemblyName || c.TypeName != clientControl.TypeName))
                            {
                                clientControls.Add(clientControl);
                            }
                        }
                    }
                }
                return clientControls;
            }

            return new ClientContolInfo[0];
        }

        public static void ReplaceIdentityObjectsInSequenceProject(this MTFSequence sequence)
        {
            sequence.ReplaceIdentityObjects();
            if (sequence.ExternalSubSequences != null)
            {
                foreach (var sequenceInfo in sequence.ExternalSubSequences)
                {
                    if (sequenceInfo.ExternalSequence != null)
                    {
                        sequenceInfo.ExternalSequence.ReplaceIdentityObjectsInSequenceProject();
                    }
                }
            }
        }

        public static IList<Guid> GetAllSequenceIds(this MTFSequence sequence)
        {
            var list = new List<Guid> { sequence.Id };
            if (sequence.ExternalSubSequences != null)
            {
                foreach (var sequenceInfo in sequence.ExternalSubSequences)
                {
                    if (sequenceInfo.ExternalSequence != null)
                    {
                        list.AddRange(sequenceInfo.ExternalSequence.GetAllSequenceIds());
                    }
                }
            }
            return list;
        }

        public static void ExtractExternalSequences(this MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> sequenceDict)
        {
            if (sequence.ExternalSubSequences != null)
            {
                foreach (var externalSequence in sequence.ExternalSubSequences)
                {
                    if (externalSequence.ExternalSequence != null)
                    {
                        sequenceDict[sequence.Id] = externalSequence;
                        if (externalSequence.ExternalSequence.ExternalSubSequences != null)
                        {
                            externalSequence.ExternalSequence.ExtractExternalSequences(sequenceDict);
                        }
                    }
                }
            }
        }

        public static void FillExternalSequences(this MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> sequenceDict)
        {
            sequence.ExternalSubSequences = sequenceDict.Where(x => x.Key == sequence.Id).Select(x => x.Value).ToList();
            foreach (var mtfExternalSequenceInfo in sequence.ExternalSubSequences)
            {
                if (mtfExternalSequenceInfo.ExternalSequence != null)
                {
                    mtfExternalSequenceInfo.ExternalSequence.FillExternalSequences(sequenceDict);
                }
            }
        }

        public static void CleanNotUsedActivities(MTFSubSequenceActivity subSequence)
        {
            if (subSequence.ExecutionType == ExecutionType.ExecuteByCase)
            {
                if (subSequence.Activities?.Count > 0)
                {
                    subSequence.Activities.Clear();
                }
            }
            else
            {
                if (subSequence.Cases?.Count > 0)
                {
                    subSequence.Cases.Clear();
                }
            }
        }
    }
}