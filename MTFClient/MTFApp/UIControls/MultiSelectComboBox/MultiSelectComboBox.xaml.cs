using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIControls.MultiSelectComboBox
{
    public delegate void FilterChangedEventHandler(object sender, EventArgs e);


    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl, INotifyPropertyChanged
    {
        private const string Select = "Select All";
        private const string Unselect = "Unselect All";
        private bool isChanged;
        private DateTime closeTime;
        private string selectAll = Select;
        private string displayMemberPath;
        private bool canNotifySelection = true;
        private IList<ComboBoxItem> filterItems;
        private bool needFillItems = false;
        private ICollection collectionToFill = null;

        public event FilterChangedEventHandler FilterChanged;

        public MultiSelectComboBox()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.MinWidth = 100;
            ClearWhenAllIsSelected = true;
        }



        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiSelectComboBox),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false, PropertyChangedCallback = ItemsChanged });

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control)
            {
                control.UnregisterEvents();
                control.FilterItems = CreateItems(e.NewValue, control.DisplayMemberPath, control.SelectedItemPath, control.NullValueString);
                if (control.needFillItems && control.collectionToFill != null)
                {
                    control.FillItems(control.collectionToFill);
                    control.NotifyPropertyChanged(nameof(IsSelected));
                    control.collectionToFill = null;
                    control.needFillItems = false;
                }
                control.NotifyPropertyChanged(nameof(ItemsAsString));
                control.RegisterEvents();
            }
        }



        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(false));



        private void UnregisterEvents()
        {
            if (filterItems != null)
            {
                foreach (var item in filterItems)
                {
                    item.PropertyChanged -= ComboBoxItemPropertyChanged;
                }
            }
        }

        private void RegisterEvents()
        {
            if (filterItems != null)
            {
                foreach (var item in filterItems)
                {
                    item.PropertyChanged += ComboBoxItemPropertyChanged;
                }
            }
        }

        void ComboBoxItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ComboBoxItem.IsSelected) && canNotifySelection)
            {
                isChanged = true;
                if (!UseMultiSelect)
                {
                    canNotifySelection = false;
                    foreach (var comboBoxItem in FilterItems.Where(x => x != sender))
                    {
                        comboBoxItem.IsSelected = false;
                    }

                    IsPopupOpen = false;
                    canNotifySelection = true;
                }
                NotifyPropertyChanged(nameof(IsSelected));
            }
        }


        public bool IsSelected => FilterItems != null && FilterItems.Any(i => i.IsSelected);



        private static ObservableCollection<ComboBoxItem> CreateItems(object input, string displayMemberPath, string selectedItemPath, string nullValueString)
        {
            var output = new ObservableCollection<ComboBoxItem>();
            var collection = input as IEnumerable;
            var useDisplayValue = !string.IsNullOrEmpty(displayMemberPath);
            var useOutputValue = !string.IsNullOrEmpty(selectedItemPath);
            if (!string.IsNullOrEmpty(nullValueString))
            {
                output.Add(new ComboBoxItem { Value = nullValueString, DisplayValue = nullValueString });
            }
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    var newItem = new ComboBoxItem { Value = item };
                    newItem.DisplayValue = useDisplayValue
                        ? GetPropertyValue(item, displayMemberPath)
                        : (newItem.Value == null ? null : newItem.Value.ToString());
                    newItem.OutputValue = useOutputValue ? GetPropertyValue(item, selectedItemPath) : newItem.Value;
                    output.Add(newItem);
                }
                return output;
            }
            return null;
        }

        private static object GetPropertyValue(object item, string propertyName)
        {
            if (item != null)
            {
                var property = item.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    return property.GetValue(item);
                }
            }
            return item;
        }



        public IList<ComboBoxItem> FilterItems
        {
            get => filterItems;
            set
            {
                filterItems = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ItemsAsString));
            }
        }

        public string ItemsAsString => FilterItems != null ? string.Join("; ", FilterItems.Where(x => x.IsSelected).Select(x => x.DisplayValue)) : null;
        //public string ReadOnlyItemsAsString => GetReadOnlyItemsAsString();

        //private string GetReadOnlyItemsAsString()
        //{
        //    if (FilterItems!=null)
        //    {
        //        var selected = FilterItems.Where(x => x.IsSelected).ToList();
        //        var displayItems = new StringBuilder();
        //        var valueItems = new StringBuilder();

        //        for (int i = 0; i < selected.Count; i++)
        //        {
        //            var item = selected[i];
        //            displayItems.Append(item.DisplayValue);
        //            valueItems.Append(item.Value);
        //            if (i<selected.Count-2)
        //            {
        //                displayItems.Append(';');
        //                valueItems.Append(';');
        //            }
        //        }

        //        return $"{displayItems} {valueItems}";
        //    }

        //    return null;
        //}


        public bool UseMultiSelect
        {
            get => (bool)GetValue(UseMultiSelectProperty);
            set => SetValue(UseMultiSelectProperty, value);
        }

        public static readonly DependencyProperty UseMultiSelectProperty =
            DependencyProperty.Register("UseMultiSelect", typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(true));




        public IEnumerable SelectedItems
        {
            get => (IEnumerable)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(MultiSelectComboBox),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true, PropertyChangedCallback = UpdateSelectedItems });

        private static void UpdateSelectedItems(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MultiSelectComboBox control))
            {
                return;
            }

            if (e.NewValue == null)
            {
                control.SetAllItems(false);
            }
            else
            {
                var collection = e.NewValue as ICollection;
                if (collection != null && control.FilterItems != null)
                {
                    control.FillItems(collection);
                }
                else
                {
                    control.needFillItems = true;
                    control.collectionToFill = collection;
                }

                control.NotifyPropertyChanged(nameof(ItemsAsString));
            }
            control.NotifyPropertyChanged(nameof(IsSelected));

        }



        private void FillItems(ICollection collection)
        {
            if (!IsPopupOpen)
            {
                foreach (var filterItem in filterItems)
                {
                    filterItem.IsSelected = false;
                }
            }
            foreach (var item in collection)
            {
                var filterItem = filterItems.FirstOrDefault(x => Equals(item, x.OutputValue));
                if (filterItem != null)
                {
                    filterItem.IsSelected = true;
                }
            }
        }


        private bool isPopupOpen;

        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                if (value)
                {
                    isChanged = false;
                }
                else
                {
                    closeTime = DateTime.Now;
                }
                if (!value && isPopupOpen && FilterItems != null && isChanged)
                {
                    var previousValue = SelectedItems;
                    SelectedItems = GetSelectedItems();
                    if (previousValue == null && SelectedItems == null)
                    {
                        SetAllItems(false);
                        NotifyPropertyChanged(nameof(ItemsAsString));
                    }

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }


                isPopupOpen = value;
                NotifyPropertyChanged();

            }
        }

        private IEnumerable GetSelectedItems()
        {
            var firstItem = FilterItems?.FirstOrDefault(x => x.OutputValue != null);
            if (firstItem != null)
            {
                var nullValue = filterItems.Any(x => x.OutputValue == null);
                var value = firstItem.OutputValue;

                if (value != null)
                {
                    return GenerateCollectionOfType(value.GetType(), FilterItems, nullValue, !ClearWhenAllIsSelected || !FilterItems.All(i => i.IsSelected));
                }
            }
            return null;
        }

        private IEnumerable GenerateCollectionOfType(Type type, IList<ComboBoxItem> items, bool useNullableType, bool fillItems)
        {
            var list = useNullableType ?
                Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(Nullable<>).MakeGenericType(type))) as IList
                : Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
            if (list != null && fillItems)
            {
                var selectedItems = items.Where(x => x.IsSelected).Select(x => x.OutputValue == null ? null : Convert.ChangeType(x.OutputValue, type));
                foreach (var selectedItem in selectedItems)
                {
                    list.Add(selectedItem);
                }
                return list;
            }
            return null;
        }


        public string DisplayMemberPath
        {
            get => displayMemberPath;
            set
            {
                displayMemberPath = value;
                SetDisplayMemberPath();
            }
        }

        private void SetDisplayMemberPath()
        {
            if (filterItems != null && !string.IsNullOrEmpty(displayMemberPath))
            {
                foreach (var item in filterItems)
                {
                    item.DisplayValue = GetPropertyValue(item.Value, displayMemberPath);
                }
            }
        }

        public string SelectedItemPath { get; set; }

        public string NullValueString { get; set; }

        public bool ClearWhenAllIsSelected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - closeTime).TotalMilliseconds > 200)//TODO better solution
            {
                IsPopupOpen = !IsPopupOpen;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            switch (selectAll)
            {
                case Unselect:
                    SetAllItems(false);
                    break;
                case Select:
                    SetAllItems(true);
                    break;
            }
            SelectAll = selectAll == Unselect ? Select : Unselect;
        }

        private void SetAllItems(bool state)
        {
            if (FilterItems != null)
            {
                foreach (var item in FilterItems)
                {
                    item.IsSelected = state;
                }
            }
        }



        public string SelectAll
        {
            get => selectAll;
            set
            {
                selectAll = value;
                NotifyPropertyChanged();
            }
        }
    }
}
