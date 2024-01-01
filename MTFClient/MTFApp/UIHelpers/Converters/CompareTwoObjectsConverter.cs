using System;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class CompareTwoObjectsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values[0] != null && values[1] != null && values[0] == values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
