using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIControls.EditableListParameter
{
    /// <summary>
    /// Interaction logic for EditableListParameter.xaml
    /// </summary>
    public partial class EditableListParameter : UserControl, INotifyPropertyChanged
    {
        private string displayValue;
        private bool isPopupOpen;
        private bool doNotHandleValueChanged;
        private bool listIsLoaded;
        private bool valueIsLoaded;
        private Type itemValueType = null;
        private bool isComplexTerm;
        private bool isValid = true;

        public EditableListParameter()
        {
            InitializeComponent();
            EditableListParameterRoot.DataContext = this;
        }

        public IEnumerable<ISemradList> ItemsSource
        {
            get => (IEnumerable<ISemradList>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<ISemradList>), typeof(EditableListParameter),
                new PropertyMetadata(OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableListParameter)d;
            editor.listIsLoaded = true;
            editor.SetItemValueType();
            editor.ValueChanged(editor.Value);
        }

        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(EditableListParameter), new PropertyMetadata(false));



        private void SetItemValueType()
        {
            itemValueType = ItemsSource?.FirstOrDefault()?.Value?.GetType();
        }

        public bool UsePlainValue { get; set; }

        public object Value
        {
            get => (object)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string DisplayValue
        {
            get => displayValue;
            set
            {
                displayValue = value;
                UpdateSourceFromDisplayValue(value);
                NotifyPropertyChanged();
            }
        }


        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                isPopupOpen = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsComplexTerm
        {
            get => isComplexTerm;
            set
            {
                isComplexTerm = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsValid
        {
            get => isValid;
            set
            {
                isValid = value;
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(EditableListParameter),
                new FrameworkPropertyMetadata
                {
                    PropertyChangedCallback = OnValueChanged,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    BindsTwoWayByDefault = true
                });

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableListParameter)d;
            editor.valueIsLoaded = true;
            editor.ValueChanged(e.NewValue);
        }

        private void UpdateSourceFromDisplayValue(string value)
        {
            doNotHandleValueChanged = true;
            if (itemValueType != null)
            {
                var adjustedValue = AdjustValueByType(value);

                IsValid = adjustedValue?.GetType() == itemValueType;

                if (IsValid)
                {
                    AssignValue(itemValueType, adjustedValue);
                }
            }
            else
            {
                AssignValue(typeof(string), value);
            }

            doNotHandleValueChanged = true;
        }

        private void AssignValue(Type constantTermType, object value)
        {
            Value = UsePlainValue ? value : new ConstantTerm(constantTermType) { Value = value };
        }

        private void ValueChanged(object newValue)
        {
            if (doNotHandleValueChanged)
            {
                return;
            }

            if (listIsLoaded && valueIsLoaded)
            {
                IsComplexTerm = newValue is Term && (!(newValue is ConstantTerm || newValue is EmptyTerm));

                if (ItemsSource != null && newValue != null)
                {
                    newValue = AdjustValueByType(newValue);

                    if (newValue != null)
                    {
                        var item = ItemsSource.FirstOrDefault(x => newValue.Equals(x.Value));

                        displayValue = item != null ? item.DisplayName : newValue.ToString();

                        NotifyPropertyChanged(nameof(DisplayValue));
                    }
                }
            }
        }

        private object AdjustValueByType(object newValue)
        {
            var tmpValue = newValue;
            if (tmpValue is ConstantTerm constantTerm)
            {
                tmpValue = constantTerm.Value;
            }

            if (itemValueType != null && itemValueType.IsValueType && tmpValue is string stringValue)
            {
                var method = itemValueType.GetMethod("TryParse",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new[] { typeof(string), itemValueType.MakeByRefType() },
                    null);

                if (method != null)
                {
                    var parameters = new object[] { stringValue, null };
                    if ((bool)method.Invoke(null, parameters))
                    {
                        return parameters[1];
                    }
                }
            }

            return tmpValue;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void OpenPopupButton(object sender, RoutedEventArgs e)
        {
            IsPopupOpen = !IsPopupOpen;
        }

        private void ListItemOnClick(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).CommandParameter is ISemradList item)
            {
                displayValue = item.DisplayName;
                NotifyPropertyChanged(nameof(DisplayValue));
                doNotHandleValueChanged = true;
                AssignValue(itemValueType ?? typeof(string), item.Value);
                doNotHandleValueChanged = false;
                IsValid = true;
            }

            IsPopupOpen = false;
        }
    }
}