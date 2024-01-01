using System.Globalization;
using AutomotiveLighting.Localization;

namespace MTFClientServerCommon.Helpers
{
    public static class LanguageHelper
    {
        public const string DefaultCulture = "en-US";

        public static Localization Localization { get; set; }

        public static void Initialize(string assembly, string assemblyFolder, string dictionary)
        {
            Localization = new Localization(assembly, assemblyFolder, dictionary, new CultureInfo(DefaultCulture));
        }

        public static void ChangeLanguage(string languageKey)
        {
            if (string.IsNullOrEmpty(languageKey))
            {
                languageKey = DefaultCulture;
            }
            try
            {
                var requiredCultureInfo = new CultureInfo(languageKey);
                Localization.SetCultureInfo(requiredCultureInfo);
                CultureInfo.DefaultThreadCurrentCulture = requiredCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = requiredCultureInfo;
                CultureInfo.CurrentCulture = requiredCultureInfo;
                CultureInfo.CurrentUICulture = requiredCultureInfo;
            }
            catch (CultureNotFoundException)
            {
                var defaultCulture = new CultureInfo(DefaultCulture);
                Localization.SetCultureInfo(defaultCulture);
                CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
                CultureInfo.CurrentCulture = defaultCulture;
                CultureInfo.CurrentUICulture = defaultCulture;
            }
        }

        public static string GetString(string key)
        {
            return Localization.GetString(key);
        }


    }
}
