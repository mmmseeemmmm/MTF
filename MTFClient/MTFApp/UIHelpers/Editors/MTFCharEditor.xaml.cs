using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFCharEditor.xaml
    /// </summary>
    public partial class MTFCharEditor : MTFEditorBase
    {
        private bool dontAcceptPropertyChanged = false;

        #region TypeName dependency property

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFCharEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });

        #endregion TypeName dependency property

        public MTFCharEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }

        private string charValue;
        public string CharValue
        {
            get { return charValue; }
            set
            {
                charValue = value;

                dontAcceptPropertyChanged = true;
                Value = ConvertToChar(value);
                dontAcceptPropertyChanged = false;

                NotifyPropertyChanged();
            }
        }

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

            CharValue = e.NewValue.ToString();
        }

        private object ConvertToChar(string value)
        {
            char ch;

            if (char.TryParse(value, out ch))
            {
                return ch;
            }

            return value;
        }
    }

    public class StringToCharValidationRule : ValidationRule
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

            string @string = value.ToString();

            if (typeInfo.IsNullableSimpleType)
            {
                if (!string.IsNullOrWhiteSpace(@string))
                {
                    if (!this.CheckStringIsChar(@string))
                    {
                        return new ValidationResult(false, "Value is not char.");
                    }
                }
            }
            else
            {
                if (!this.CheckStringIsChar(@string))
                {
                    return new ValidationResult(false, "Value is not char.");
                }
            }
            
            return new ValidationResult(true, null);
        }

        private bool CheckStringIsChar(string value)
        {
            char ch;

            bool isChar = char.TryParse(value, out ch);

            return isChar;
        }
    }
}
