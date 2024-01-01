using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    /// <summary>
    /// Interaction logic for LocalizedStringEditor.xaml
    /// </summary>
    public partial class LocalizedStringEditor : LocalizedStringBase
    {
        private Dictionary<string, string> hints;
        private string displayValue;
        private bool updateDisplayValue = true;
        private bool updateKey = true;
        private bool openHints;
        private bool handleSelection = true;

        public LocalizedStringEditor()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        #region Properties

        public Dictionary<string, string> Hints
        {
            get { return hints; }
            set
            {
                hints = value;
                NotifyPropertyChanged();
            }
        }

        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;// UniqueIndexer>0? AdjustName(value, UniqueIndexer): value;
                if (value != null && updateKey)
                {
                    updateDisplayValue = false;
                    GenerateHints(value);
                    updateDisplayValue = true;
                }
                NotifyPropertyChanged();
            }
        }

        public bool OpenHints
        {
            get { return openHints; }
            set
            {
                openHints = value;
                NotifyPropertyChanged();
            }
        }
        
        #endregion

        #region override

        protected override void OnIdentifierChanged(object newValue)
        {
            if (updateDisplayValue && newValue != null)
            {
                updateKey = false;
                AssignDisplayValue((string)newValue);
                updateKey = true;
            }
        }

        protected override void OnUniqueIndexerChanged(int newValue)
        {
            if (newValue>0 && updateDisplayValue)
            {
                displayValue = AdjustName(displayValue, newValue);
                NotifyPropertyChanged("DisplayValue");
            }
        }

        public override void SetLocTextExplicit()
        {
            var translatedValue = ActualDictionary.GetValue(Identifier);
            displayValue = UniqueIndexer > 0 ? AdjustName(translatedValue, UniqueIndexer) : translatedValue;
            NotifyPropertyChanged("DisplayValue");
        }


        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (OpenHints)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        {
                            if (HintsListBox.SelectedIndex < HintsListBox.Items.Count)
                            {
                                ChangeIndex(HintsListBox.SelectedIndex + 1);
                            }
                            break;
                        }

                    case Key.Up:
                        {
                            if (HintsListBox.SelectedIndex > 0)
                            {
                                ChangeIndex(HintsListBox.SelectedIndex - 1);
                            }
                            break;
                        }
                    case Key.Enter:
                    case Key.Tab:
                        {
                            AssignSelection(true);
                            break;
                        }

                    case Key.Escape:
                        {
                            OpenHints = false;
                            break;
                        }
                }
            }
        }

        #endregion

        #region private methods

        private void AssignDisplayValue(string key)
        {
            DisplayValue = ActualDictionary.GetValue(key);
        }

        private void GenerateHints(string newValue)
        {
            Hints = ActualDictionary.GetHints(newValue);
            if (Hints != null)
            {
                OpenHints = true;
                var matchKey = Hints.FirstOrDefault(x =>
                {
                    var x1 = x.Value.GetNonPunctionalLowerString();
                    var x2 = newValue.GetNonPunctionalLowerString();
                    return x1 != null && x1.Equals(x2);
                }).Key;
                Identifier = matchKey ?? newValue;
            }
        }


        private void HintsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!handleSelection)
            {
                return;
            }
            AssignSelection();
        }

        private void AssignSelection(bool moveCursor = false)
        {
            if (HintsListBox.SelectedItem != null)
            {
                var currentItem = (KeyValuePair<string, string>)HintsListBox.SelectedItem;
                if (currentItem.Key != null)
                {
                    Identifier = currentItem.Key;
                }
            }

            OpenHints = false;

            if (moveCursor)
            {
                SearchField.CaretIndex = SearchField.Text.Length;
            }
        }

        private void ChangeIndex(int newIndex)
        {
            handleSelection = false;
            HintsListBox.SelectedIndex = newIndex;
            HintsListBox.ScrollIntoView(HintsListBox.SelectedItem);
            handleSelection = true;
        }

        #endregion
    }
}
