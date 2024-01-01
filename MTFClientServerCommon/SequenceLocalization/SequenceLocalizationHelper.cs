using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.SequenceLocalization
{
    public static class SequenceLocalizationHelper
    {
        private static readonly List<string> availableLocalizations = new List<string> { "cs-CZ", "de-DE", "it-IT" };
        private static readonly List<string> exceptionKeys = new List<string>();
        private static NameDictionary actualDictionary;

        public static NameDictionary ActualDictionary
        {
            get { return actualDictionary; }
        }

        public static List<string> ExceptionKeys
        {
            get { return exceptionKeys; }
        }


        private static void FillDictionary()
        {
            var defaultCulture = LanguageHelper.DefaultCulture;
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var cultureToLoad = availableLocalizations.Contains(currentCulture.IetfLanguageTag) ? currentCulture.IetfLanguageTag : defaultCulture;

            var rm = new ResourceManager(typeof(ActivityNames));
            var set = rm.GetResourceSet(new CultureInfo(cultureToLoad), true, true);

            actualDictionary = new NameDictionary(cultureToLoad);
            foreach (DictionaryEntry entry in set)
            {
                actualDictionary.Add(entry.Key.ToString(), entry.Value.ToString());
            }
        }

        public static void Load()
        {
            FillExceptionKeys();
            FillDictionary();
        }

        private static void FillExceptionKeys()
        {
            exceptionKeys.Clear();
            exceptionKeys.Add(ActivityNameConstants.CallWholeSequenceKey);
        }

        public static string TranslateActivityName(this MTFSequenceActivity activity)
        {
            if (activity != null && ActualDictionary != null)
            {
                return MTFSequenceActivityHelper.CombineTranslatedActivityName(ActualDictionary.GetValue(activity.ActivityName), activity.UniqueIndexer);
            }
            return null;
        }

        public static string TranslateActivityPath(List<ActivityIdentifier> activityPath)
        {
            return TranslateActivityPath(activityPath, ActivityNameConstants.ActivityPathSeparator);
        }

        public static string TranslateActivityPath(List<ActivityIdentifier> activityPath, string separator)
        {
            var sb = new StringBuilder();
            if (activityPath!=null)
            {
                for (int i = 0; i < activityPath.Count-1; i++)
                {
                    var item = activityPath[i];
                    sb.Append(MTFSequenceActivityHelper.CombineTranslatedActivityName(ActualDictionary.GetValue(item.ActivityKey),
                        item.UniqueIndexer));
                    sb.Append(separator);
                }
                sb.Append(MTFSequenceActivityHelper.CombineTranslatedActivityName(ActualDictionary.GetValue(activityPath.Last().ActivityKey),
                        activityPath.Last().UniqueIndexer));
            }
            return sb.ToString();
        }

    }

    public class NameDictionary
    {
        private readonly string languageKey;
        private readonly Dictionary<string, string> values;

        public NameDictionary(string language)
        {
            languageKey = language;
            values = new Dictionary<string, string>();
        }

        public string LanguageKey
        {
            get { return languageKey; }
        }

        public void Add(string key, string value)
        {
            values[key] = value;
        }

        public Dictionary<string, string> GetHints(string value)
        {
            return values.Where(x =>
                                {
                                    if (!SequenceLocalizationHelper.ExceptionKeys.Contains(x.Key))
                                    {
                                        var s1 = x.Value.GetNonPunctionalLowerString();
                                        var s2 = value.GetNonPunctionalLowerString();
                                        return s1.Contains(s2);
                                    }
                                    return false;
                                }
                ).Take(20).ToDictionary(x => x.Key, y => y.Value);
        }

        public string GetValue(string key)
        {
            return values.ContainsKey(key) ? values[key] : key;
        }

        public bool Contains(string key)
        {
            return values.ContainsKey(key);
        }

        public string GetKeyByValue(string value)
        {
            return values.FirstOrDefault(x => x.Value == value).Key ?? value;
        }
    }


}
