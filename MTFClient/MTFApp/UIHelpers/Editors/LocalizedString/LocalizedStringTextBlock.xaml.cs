namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    /// <summary>
    /// Interaction logic for LocalizedStringTextBlock.xaml
    /// </summary>
    public partial class LocalizedStringTextBlock : LocalizedStringTextBlockBase
    {
        private string displayValue;

        public LocalizedStringTextBlock()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;
                NotifyPropertyChanged();
            }
        }

        private void RefreshValue(string value)
        {
            if (value != null)
            {
                var translatedValue = ActualDictionary.GetValue(value);
                DisplayValue = UniqueIndexer > 0 ? AdjustName(translatedValue, UniqueIndexer) : translatedValue;
            }
        }

        protected override void OnIdentifierChanged(object newValue)
        {
            RefreshValue(newValue as string);
        }

        protected override void OnUniqueIndexerChanged(int newValue)
        {
            RefreshValue(Identifier);
        }

        public override void SetLocTextExplicit()
        {
            RefreshValue(Identifier);
        }
    }
}
