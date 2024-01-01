using System;
using System.Globalization;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class EnumToBoleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
