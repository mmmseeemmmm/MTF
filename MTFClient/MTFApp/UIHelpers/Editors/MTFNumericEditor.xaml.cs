using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFNumericEditor.xaml
    /// </summary>
    public partial class MTFNumericEditor : MTFEditorBase
    {
        public MTFNumericEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }

        #region TypeName dependency property
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFNumericEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
        #endregion TypeName dependency property

        private string stringValue;
        public string StringValue
        {
            get { return stringValue; }
            set
            {
                stringValue = value;
                
                dontAcceptPropertyChanged = true;
                Value = ConvertToType(value);
                dontAcceptPropertyChanged = false;

                NotifyPropertyChanged();
            }
        }

        private bool dontAcceptPropertyChanged = false;
        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property != ValueProperty || dontAcceptPropertyChanged)
            {
                return;
            }

            base.OnPropertyChanged(source, e);

            if (e.NewValue == null)
            {
                return;
            }

            var toStringMethod = e.NewValue.GetType().GetMethod("ToString", new Type[] { typeof(string) });
            if (toStringMethod != null)
            {
                var s = (string)toStringMethod.Invoke(e.NewValue, new object[] { "0.############################################" });
                StringValue = s;
            }
            else
            {
                StringValue = e.NewValue.ToString();
            }
        }

        private object ConvertToType(string value)
        {
            if (TypeName==null)
            {
                return value;
            }
            Type numericType = Type.GetType(TypeName);
            if (numericType == null)
            {
                return value;
            }
            
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            value = value.Replace(",", decimalSeparator);
            value = value.Replace(".", decimalSeparator);

            object[] args = new object[] { value.ToString(), null };

            var typeInfo = new TypeInfo(TypeName);

            if (typeInfo.IsNullableSimpleType)
            {
                if ((bool)typeInfo.InnerTypeInfo.Type.GetMethod("TryParse", new[] { typeof(string), typeInfo.InnerTypeInfo.Type.MakeByRefType() }).Invoke(null, args))
                {
                    return args[1];
                }    
            }
            else
            {
                if ((bool)numericType.GetMethod("TryParse", new[] { typeof(string), numericType.MakeByRefType() }).Invoke(null, args))
                {
                    return args[1];
                }    
            }

            return value;
        }
    }

    public class StringToNumericValidationRule : ValidationRule
    {
        public StringContainer TypeNameContainer
        {
            get;
            set;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value is null.");
            }

            var typeInfo = new TypeInfo(TypeNameContainer.Value);

            if (typeInfo.Type == null)
            {
                return new ValidationResult(false, "Type name is invalid.");
            }

            try
            {
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string @string = value.ToString();

                @string = @string.Replace(",", decimalSeparator);
                @string = @string.Replace(".", decimalSeparator);

                if (typeInfo.IsNullableSimpleType)
                {
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        if (!this.CheckIsNumeric(typeInfo.InnerTypeInfo.Type, @string))
                        {
                            return new ValidationResult(false, "Value is not numeric.");
                        }
                    }
                }
                else
                {
                    if (!this.CheckIsNumeric(typeInfo.Type, @string))
                    {
                        return new ValidationResult(false, "Value is not numeric.");
                    }
                }
            }
            catch(Exception exception)
            {
                return new ValidationResult(false, exception.Message);
            }

            return new ValidationResult(true, null);    
        }

        private bool CheckIsNumeric(Type type, string value)
        {
            bool isNumeric = (bool)type.GetMethod("TryParse", new[] { typeof(string), type.MakeByRefType() }).Invoke(null, new object[] { value, null });

            return isNumeric;
        }
    }

    public class StringContainer : FrameworkElement
    {
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(StringContainer));
    }

}
