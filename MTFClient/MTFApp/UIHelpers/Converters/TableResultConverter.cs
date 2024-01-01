using System;
using System.Globalization;
using System.Windows.Data;
using MTFCommon;

namespace MTFApp.UIHelpers.Converters
{
    class TableResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MTFValidationTableStatus status;
            if (value != null && Enum.TryParse(value.ToString(), out status))
            {
                return status;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
