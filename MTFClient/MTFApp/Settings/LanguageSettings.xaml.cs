using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutomotiveLighting.MTFCommon;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for LanguageSettings.xaml
    /// </summary>
    public partial class LanguageSettings : UserControl, INotifyPropertyChanged
    {
        private List<MTFLanguage> languages;

        public LanguageSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
            Task.Run(() => GenerateAvailableLanguages());
        }

        public List<MTFLanguage> Languages => languages;

        private void GenerateAvailableLanguages()
        {
            var list = new List<MTFLanguage>
                       {
                           new MTFLanguage
                           {
                               Key = "en-US",
                               Name = "English",
                               Icon = MTFIcons.LanguageEn
                           },
                           new MTFLanguage
                           {
                               Key = "cs-CZ",
                               Name = "Česky",
                               Icon = MTFIcons.LanguageCz
                           },
                           new MTFLanguage
                           {
                               Key = "de-DE",
                               Name = "Deutsch",
                               Icon = MTFIcons.LanguageDe
                           },
                           new MTFLanguage
                           {
                               Key = "it-IT",
                               Name = "Italiano",
                               Icon = MTFIcons.LanguageIt
                           },
                           new MTFLanguage
                           {
                               Key = "es-MX",
                               Name = "Español",
                               Icon = MTFIcons.LanguageMx
                           },
                       };
            languages = list;
            NotifyPropertyChanged("Languages");
        }


        public string CurrentLanguage
        {
            get => (string)GetValue(CurrentLanguageProperty);
            set => SetValue(CurrentLanguageProperty, value);
        }

        public static readonly DependencyProperty CurrentLanguageProperty =
            DependencyProperty.Register("CurrentLanguage", typeof(string), typeof(LanguageSettings),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = true,
                });


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
    }
}