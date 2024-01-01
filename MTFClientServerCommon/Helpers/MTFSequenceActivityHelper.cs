using System;
using System.Collections.Generic;
using System.Text;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon.Helpers
{
    public static class MTFSequenceActivityHelper
    {
        public static string GetActivityPath(this MTFSequenceActivity activity)
        {
            StringBuilder sb = new StringBuilder();
            GeneratePath(activity, sb);
            //sb.Append(activity.ActivityName);
            return sb.ToString();
        }

        public static List<Guid> GetActivityGuidPath(this MTFSequenceActivity activity)
        {
            var list = new List<Guid>();
            GenerateGuidPath(activity, list);
            list.Reverse();
            return list;
        }

        private static void GenerateGuidPath(MTFDataTransferObject dto, List<Guid> list)
        {
            var activity = dto as MTFSequenceActivity;
            if (activity != null)
            {
                list.Add(activity.Id);
                GenerateGuidPath(activity.Parent, list);
            }
            else
            {
                var @case = dto as MTFCase;
                if (@case != null)
                {
                    list.Add(@case.Id);
                    GenerateGuidPath(@case.Parent, list);
                }
                else
                {
                    var sequence = dto as MTFSequence;
                    if (sequence != null)
                    {
                        list.Add(sequence.Id);
                    }
                }
            }
        }


        private static string GeneratePath(MTFSequenceActivity activity, StringBuilder sb)
        {
            if (activity.Parent is MTFSequenceActivity)
            {
                sb.Append(GeneratePath(activity.Parent as MTFSequenceActivity, sb));
                sb.Append("\\");
            }
            else
            {
                var mtfCase = activity.Parent as MTFCase;
                if (mtfCase != null)
                {
                    sb.Append(GeneratePath(mtfCase.Parent as MTFSequenceActivity, sb));
                    sb.Append(mtfCase.Name);
                    sb.Append("\\");
                    return mtfCase.Name;
                }
            }
            return CombineTranslatedActivityName(activity.ActivityName, activity.UniqueIndexer);
        }

        public static void ExpandParentSubSequence(this MTFSequenceActivity activity)
        {
            var subSequenceActivity = activity.Parent as MTFSubSequenceActivity;
            if (subSequenceActivity != null)
            {
                if (subSequenceActivity.IsCollapsed)
                {
                    subSequenceActivity.IsCollapsed = false;
                }
                subSequenceActivity.ExpandParentSubSequence();
            }
            else
            {
                var mtfCase = activity.Parent as MTFCase;
                if (mtfCase != null)
                {
                    var subSequenceCase = mtfCase.Parent as MTFSubSequenceActivity;
                    if (subSequenceCase != null)
                    {
                        if (subSequenceCase.IsCollapsed)
                        {
                            subSequenceCase.IsCollapsed = false;
                        }
                        if (subSequenceCase.Cases != null)
                        {
                            subSequenceCase.ActualCaseIndex = subSequenceCase.Cases.IndexOf(mtfCase);
                        }
                        subSequenceCase.ExpandParentSubSequence();
                    }
                }
            }
        }

        public static List<ActivityIdentifier> GenerateShortReportPath(this MTFSequenceActivity activity)
        {
            return GenerateShortReportPath(activity, 4);
        }

        public static List<ActivityIdentifier> GenerateShortReportPath(this MTFSequenceActivity activity, int level)
        {
            if (activity == null)
            {
                return null;
            }

            var activityNames = new List<ActivityIdentifier>();
            NamePath(activity, activityNames, level);

            activityNames.Reverse();

            return activityNames;
        }

        private static void NamePath(MTFSequenceActivity activity, List<ActivityIdentifier> activityNames, int level)
        {
            if (activity == null || level == 0)
            {
                return;
            }

            activityNames.Add(new ActivityIdentifier { ActivityKey = activity.ActivityName, UniqueIndexer = activity.UniqueIndexer });
            NamePath(activity.GetParent<MTFSubSequenceActivity>(), activityNames, level - 1);
        }

        public static string CombineTranslatedActivityName(string translatedName, int indexer)
        {
            return indexer > 0 ? string.Format("{0}{2}{1:00}", translatedName, indexer, ActivityNameConstants.ActivityIndexerSeparator) : translatedName;
        }

        public static ISequenceClassInfo GetParent(MTFDataTransferObject obj)
        {
            if (obj.Parent == null)
            {
                return null;
            }

            if (obj.Parent is ISequenceClassInfo classInfo)
            {
                return classInfo;
            }

            return GetParent(obj.Parent);
        }
    }
}
