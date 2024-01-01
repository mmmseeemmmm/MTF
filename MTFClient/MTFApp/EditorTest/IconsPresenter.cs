using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace MTFApp.EditorTest
{
    class IconsPresenter
    {
        public  List<IconStyle> MTFIcons => mtfIcons ?? (mtfIcons = GetIconsByResourceDictionary(@"Resources/MTFIcons/"));
        public  List<IconStyle> CommandIcons => commandIcons ?? (commandIcons = GetIconsByResourceDictionary(@"Resources/CommandIcons/"));
        public  List<IconStyle> LocalizationIcons => localizationIcons ?? (localizationIcons = GetIconsByResourceDictionary(@"Localizations/Icons/"));

        private List<IconStyle> mtfIcons = null;
        private List<IconStyle> commandIcons = null;
        private List<IconStyle> localizationIcons = null;

        private List<IconStyle> GetIconsByResourceDictionary(string resourceDirName)
        {
            var icons = new List<IconStyle>();
            var appResDict = Application.Current.Resources;

            foreach (var mergedDir in appResDict.MergedDictionaries)
            {
                if (mergedDir.Source.ToString().StartsWith(resourceDirName))
                {
                    foreach (DictionaryEntry dictEntry in mergedDir)
                    {
                        icons.Add(new IconStyle
                        {
                            Name = dictEntry.Key.ToString(),
                            Style = dictEntry.Value as Style,
                        });
                    }
                }
            }

            return icons;
        }
    }

    class IconStyle
    {
        public string Name { get; set; }
        public Style Style { get; set; }
    }
}
