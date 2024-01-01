using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.UIHelpers
{
    public static class UniqueNameHeleper
    {
        public static void AdjustName<T>(this T item, IEnumerable<T> collection) where T : MTFDataTransferObject
        {
            if (item is MTFVariable)
            {
                (item as MTFVariable).AdjustName(collection as IEnumerable<MTFVariable>);
            }
            else if (item is MTFSequenceClassInfo)
            {
                (item as MTFSequenceClassInfo).AdjustName(collection as IEnumerable<MTFSequenceClassInfo>);
            }
            else if (item is MTFClassInstanceConfiguration)
            {
                (item as MTFClassInstanceConfiguration).AdjustName(collection as IEnumerable<MTFClassInstanceConfiguration>);
            }
            else if (item is MTFSequenceActivity)
            {
                (item as MTFSequenceActivity).AdjustName();
            }
            else if (item is MTFServiceCommand)
            {
                (item as MTFServiceCommand).AdjustName(collection as IEnumerable<MTFServiceCommand>);
            }
        }
        public static void AdjustName(this MTFSequenceClassInfo classInfo, IEnumerable<MTFSequenceClassInfo> collection)
        {
            var name = classInfo.Alias;
            if (string.IsNullOrEmpty(name))
            {
                classInfo.Alias = classInfo.MTFClass.Name;
            }
            else
            {
                while (FindName(name, classInfo.Id, collection))
                {
                    int o;
                    name = AdjustName(name, out o);
                }
                classInfo.Alias = name;
            }
        }

        public static void AdjustName(this MTFServiceCommand serviceCommand, IEnumerable<MTFServiceCommand> collection)
        {
            if (collection == null)
            {
                return;
            }
            var name = serviceCommand.Name;
            if (string.IsNullOrEmpty(name))
            {
                serviceCommand.Name = "New Service Command";
            }
            else
            {
                while (FindName(name, serviceCommand.Id, collection))
                {
                    int o;
                    name = AdjustName(name, out o);
                }
                serviceCommand.Name = name;
            }
        }

        public static void AdjustName(this MTFUserCommand serviceCommand, IEnumerable<MTFUserCommand> collection)
        {
            if (collection == null)
            {
                return;
            }
            var name = serviceCommand.Name;
            if (string.IsNullOrEmpty(name))
            {
                serviceCommand.Name = "New User Command";
            }
            else
            {
                while (FindName(name, serviceCommand.Id, collection))
                {
                    int o;
                    name = AdjustName(name, out o);
                }
                serviceCommand.Name = name;
            }
        }

        public static void AdjustName(this MTFVariable variable, IEnumerable<MTFVariable> collection)
        {
            var name = variable.Name;
            if (string.IsNullOrEmpty(name))
            {
                string typeName = string.Empty;
                var memberInfo = Type.GetType(variable.TypeName);
                if (memberInfo != null)
                {
                    typeName = memberInfo.Name;
                }
                variable.Name = string.Format("New {0} Variable", typeName);
            }
            else
            {
                while (FindName(name, variable.Id, collection))
                {
                    int o;
                    name = AdjustName(name, out o);
                }
                variable.Name = name;
            }
            if (variable.Value is IMTFTable)
            {
                ((IMTFTable)variable.Value).Name = variable.Name;
            }
        }

        public static void AdjustName(this MTFClassInstanceConfiguration classInstanceConfiguration, IEnumerable<MTFClassInstanceConfiguration> collection)
        {
            var name = classInstanceConfiguration.Name;
            while (FindName(name, classInstanceConfiguration.Id, collection))
            {
                int o;
                name = AdjustName(name, out o);
            }
            classInstanceConfiguration.Name = name;
        }

        public static void AdjustName(this MTFSequenceActivity activity)
        {
            if (activity!=null)
            {
                var translatedName = SequenceLocalizationHelper.ActualDictionary.GetValue(activity.ActivityName);
                var tmp = MTFSequenceActivityHelper.CombineTranslatedActivityName(translatedName, activity.UniqueIndexer);
                var collection = GetCollection(activity);
                if (collection!= null && IsDuplicity(tmp, activity.Id, collection))
                {
                    activity.UniqueIndexer = GetFreeIndexer(activity, collection);
                }
            }
        }

        private static IList<MTFSequenceActivity> GetCollection(MTFSequenceActivity activity)
        {
            object parent = null;
            if (activity != null)
            {
                parent = activity.Parent;
            }

            switch (parent)
            {
                case MTFSequence sequence:
                    return sequence.ActivitiesByCall.Any(x => x.Id == activity.Id) ? sequence.ActivitiesByCall : sequence.MTFSequenceActivities;
                case MTFSubSequenceActivity subSequence:
                    return subSequence.Activities;
                case MTFCase @case:
                    return @case.Activities;
                case MTFExecuteActivity executeActivity:
                    var s = executeActivity.GetParent<MTFSequence>();
                    return s.ActivitiesByCall.Any(x => x.Id == activity.Id) ? s.ActivitiesByCall : s.MTFSequenceActivities;
                default:
                    return null;
            }
        }

        private static int GetFreeIndexer(MTFSequenceActivity activity, IList<MTFSequenceActivity> collection)
        {
            int i = 0;
            var activityName = activity.ActivityName;

            bool isKey = SequenceLocalizationHelper.ActualDictionary.Contains(activityName);

            if (!isKey && activityName.Contains(ActivityNameConstants.ActivityIndexerSeparator))
            {
                var tmpName = activityName.Substring(0, activityName.LastIndexOf(ActivityNameConstants.ActivityIndexerSeparator, StringComparison.Ordinal));
                var key = SequenceLocalizationHelper.ActualDictionary.GetKeyByValue(tmpName);
                activityName = key;
                activity.ActivityName = key;
            }



            var translatedName = SequenceLocalizationHelper.ActualDictionary.GetValue(activityName);
            var newName = translatedName;

            while (IsDuplicity(newName, activity.Id, collection))
            {
                i++;
                newName = MTFSequenceActivityHelper.CombineTranslatedActivityName(translatedName, i);
            }

            return i;
        }

        private static bool IsDuplicity(string name, Guid id, IList<MTFSequenceActivity> activities)
        {
            return activities.Any(x => x.Id != id && x.TranslateActivityName() == name);
        }

        private static bool FindName(string name, Guid id, IEnumerable<MTFSequenceClassInfo> collection)
        {
            return collection.Any(x => x.Alias == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, IEnumerable<MTFVariable> collection)
        {
            return collection.Any(x => x.Name == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, IEnumerable<MTFServiceCommand> collection)
        {
            return collection.Any(x => x.Name == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, IEnumerable<MTFUserCommand> collection)
        {
            return collection.Any(x => x.Name == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, MTFSequence sequence)
        {
            return sequence.ActivitiesByCall.Any(x => x.ActivityName == name && x.Id != id)
                || sequence.MTFSequenceActivities.Any(x => x.ActivityName == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, IEnumerable<MTFClassInstanceConfiguration> collection)
        {
            return collection.Any(x => x.Name == name && x.Id != id);
        }

        private static bool FindName(string name, Guid id, IList<MTFSequenceActivity> activities)
        {
            return activities.Any(x => x.ActivityName == name && x.Id != id);
        }


        public static string AdjustName(string name, out int i)
        {
            i = 1;
            const char separator = '_';
            var newName = new StringBuilder();
            newName.Append(name);
            if (name.Contains(separator))
            {
                var suffix = name.Substring(name.LastIndexOf('_') + 1);
                int index;
                if (int.TryParse(suffix, out index))
                {
                    index++;
                    string number = index > 9 ? index.ToString() : "0" + index.ToString();
                    name = name.Substring(0, name.LastIndexOf(separator) + 1) + number.ToString();
                    newName.Clear();
                    newName.Append(name.Substring(0, name.LastIndexOf(separator) + 1));
                    newName.Append(number);
                    i = index;
                }
                else
                {
                    newName.Append(separator);
                    if (i < 10)
                    {
                        newName.Append("0");
                    }
                    newName.Append(i);
                }
            }
            else
            {
                newName.Append(separator);
                if (i < 10)
                {
                    newName.Append("0");
                }
                newName.Append(i);
            }
            return newName.ToString();
        }
    }
}
