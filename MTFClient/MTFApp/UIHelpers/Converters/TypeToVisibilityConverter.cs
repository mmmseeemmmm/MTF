using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class TypeToVisibilityConverter: IValueConverter
    {
        public TypeToVisibilityConverter()
        {
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
            TypesToHide = string.Empty;
        }
        public Visibility TrueValue { get; set; }

        public Visibility FalseValue { get; set; }

        public string TypesToHide { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = string.Empty;
            if (value==null)
            {
                return TrueValue;
            }
            if (value!=null && value.GetType()!=typeof(string))
            {
                val = value.GetType().FullName;
            }
            else
            {
                val = value.ToString();
            }
            if (TypesToHide.Contains(val))
            {
                return TrueValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
