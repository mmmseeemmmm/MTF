using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFVariantEditor.xaml
    /// </summary>
    public partial class MTFVariantEditor : MTFEditorBase
    {
        private List<SequenceVariantGroupContainer> groups;
        private SequenceVariant sequenceVariant = new SequenceVariant();
        private bool isLoadedSelections;
        private bool isLoadedValues;
        private bool canUpdateSelections = true;
        private bool useHorizontalVariants;
        private bool useActivityResult;
        private bool canUpdateSource = true;

        public MTFVariantEditor()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public bool UseHorizontalVariants
        {
            get => useHorizontalVariants;
            set
            {
                useHorizontalVariants = value;
                NotifyPropertyChanged();
            }
        }

        public bool UseActivityResult
        {
            get => useActivityResult;
            set
            {
                useActivityResult = value;
                NotifyPropertyChanged();
            }
        }

        public SequenceVariant SequenceVariant
        {
            get => sequenceVariant;
            set => sequenceVariant = value;
        }



        public bool AllowMultiSelect
        {
            get => (bool)GetValue(AllowMultiSelectProperty);
            set => SetValue(AllowMultiSelectProperty, value);
        }

        public static readonly DependencyProperty AllowMultiSelectProperty =
            DependencyProperty.Register("AllowMultiSelect", typeof(bool), typeof(MTFVariantEditor), new PropertyMetadata(true));



        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ParentSequenceProperty)
            {
                if (e.NewValue is MTFSequence sequence)
                {
                    if (Groups != null)
                    {
                        UnregisterEvents();
                    }
                    Groups = GenerateGroups(sequence.VariantGroups);
                    isLoadedValues = true;
                    UpdateSelections();
                }
            }
            else if (e.Property == ValueProperty)
            {
                var newData = e.NewValue as SequenceVariant;
                if (canUpdateSelections)
                {
                    isLoadedSelections = true;
                    if (newData != null)
                    {
                        sequenceVariant = newData;
                        UpdateSelections();
                    }
                    else
                    {
                        sequenceVariant = new SequenceVariant();
                        ClearSelections();
                    }
                }
            }
            base.OnPropertyChanged(source, e);
        }

        private void ClearSelections()
        {
            if (groups != null)
            {
                foreach (var groupContainer in groups)
                {
                    groupContainer.SelectedValues = null;
                }
            }
        }

        private void UnregisterEvents()
        {
            foreach (var sequenceVariantGroupContainer in Groups)
            {
                sequenceVariantGroupContainer.PropertyChanged -= VariantChanged;
            }
        }

        private void UpdateSelections()
        {
            if (canUpdateSelections && isLoadedValues && isLoadedSelections)
            {
                foreach (var variantGroupValue in sequenceVariant.VariantGroups)
                {
                    var group = Groups.FirstOrDefault(g => g.Name == variantGroupValue.Name);
                    if (group != null)
                    {
                        var value = variantGroupValue;
                        canUpdateSource = false;
                        group.SelectedValues = group.ValuesItemSource.Where(x => value.Values != null && value.Values.Any(v => v.Name == x.Name)).ToList();
                        group.Term = value.Term;
                        canUpdateSource = true;
                    }
                }
            }
        }

        private List<SequenceVariantGroupContainer> GenerateGroups(IList<SequenceVariantGroup> variantGroups)
        {
            var output = new List<SequenceVariantGroupContainer>();
            if (variantGroups != null)
            {
                int i = 0;
                foreach (var variantGroup in variantGroups)
                {
                    var data = new SequenceVariantGroupContainer
                               {
                                   Name = variantGroup.Name,
                                   ValuesItemSource = variantGroup.Values,
                                   Index = i
                               };
                    data.PropertyChanged += VariantChanged;
                    output.Add(data);
                    i++;
                }
            }
            return output;
        }

        private void VariantChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!canUpdateSource)
            {
                return;
            }

            if (e.PropertyName == nameof(SequenceVariantGroupContainer.SelectedValues))
            {
                if (sender is SequenceVariantGroupContainer currentVariant)
                {
                    sequenceVariant.SetVariant(currentVariant.Name, currentVariant.Index, currentVariant.SelectedValues);
                    UpdateVariant();
                }
            }
            else if (e.PropertyName == nameof(SequenceVariantGroupContainer.Term))
            {
                if (sender is SequenceVariantGroupContainer currentVariant)
                {
                    sequenceVariant.SetVariant(currentVariant.Name, currentVariant.Index, currentVariant.Term);
                    UpdateVariant();
                }
            }

        }

        public List<SequenceVariantGroupContainer> Groups
        {
            get => groups;
            set
            {
                groups = value;
                NotifyPropertyChanged();
            }
        }

        private void RemoveActivityResult_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.CommandParameter is SequenceVariantGroupContainer currentVariant)
                {
                    currentVariant.Term = null;
                    sequenceVariant.SetVariant(currentVariant.Name, currentVariant.Index, currentVariant.Term);
                    UpdateVariant();
                }
            }
        }

        private void UpdateVariant()
        {
            canUpdateSelections = false;
            Value = null;
            this.Value = sequenceVariant;
            canUpdateSelections = true;
        }
    }

    public class SequenceVariantGroupContainer : NotifyPropertyBase
    {
        private IEnumerable<SequenceVariantValue> selectedValues;
        private bool isActivityResult;
        private Term term = new EmptyTerm();

        public string Name { get; set; }

        public int Index { get; set; }

        public IEnumerable<SequenceVariantValue> SelectedValues
        {
            get => selectedValues;
            set
            {
                selectedValues = value;
                NotifyPropertyChanged();
            }
        }

        public IList<SequenceVariantValue> ValuesItemSource { get; set; }

        public bool IsActivityResult
        {
            get => isActivityResult;
            set
            {
                isActivityResult = value;
                NotifyPropertyChanged();
            }
        }

        public Term Term
        {
            get => term;
            set
            {
                term = value;
                IsActivityResult = !(value == null || value is EmptyTerm);
                NotifyPropertyChanged();
            }
        }
    }
}
