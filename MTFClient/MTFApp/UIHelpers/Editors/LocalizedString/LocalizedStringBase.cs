using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ALControls;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    public abstract class LocalizedStringBase : UserControl, INotifyPropertyChanged, ILocTextKeyExplicit
    {
        protected NameDictionary ActualDictionary
        {
            get { return SequenceLocalizationHelper.ActualDictionary; }
        }

        public string Identifier
        {
            get { return (string)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }

        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string), typeof(LocalizedStringBase), new FrameworkPropertyMetadata(KeyChanged) { BindsTwoWayByDefault = true });

        private static void KeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (LocalizedStringBase)d;
            editor.OnIdentifierChanged(e.NewValue);
        }


        public int UniqueIndexer
        {
            get { return (int)GetValue(UniqueIndexerProperty); }
            set { SetValue(UniqueIndexerProperty, value); }
        }

        public static readonly DependencyProperty UniqueIndexerProperty =
            DependencyProperty.Register("UniqueIndexer", typeof(int), typeof(LocalizedStringBase), new FrameworkPropertyMetadata(IndexerChanged));

        private static void IndexerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (LocalizedStringBase)d;
            editor.OnUniqueIndexerChanged((int)e.NewValue);
        }

        protected string AdjustName(string displayName, int indexer)
        {
            return MTFSequenceActivityHelper.CombineTranslatedActivityName(displayName, indexer);
        }

        protected abstract void OnIdentifierChanged(object newValue);
        protected abstract void OnUniqueIndexerChanged(int newValue);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        public abstract void SetLocTextExplicit();
    }
}
