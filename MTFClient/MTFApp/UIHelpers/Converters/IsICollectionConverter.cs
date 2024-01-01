using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class IsICollectionConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ICollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
