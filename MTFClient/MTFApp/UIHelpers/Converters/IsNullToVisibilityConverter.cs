using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class IsNullToVisibilityConverter : IValueConverter
    {
        public IsNullToVisibilityConverter()
        {
            IsNotNullValue = Visibility.Collapsed;
            IsNullValue = Visibility.Visible;
        }

        public Visibility IsNullValue { get; set; }

        public Visibility IsNotNullValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var list = value as ICollection;
            if (list != null)
            {
                return list.Count == 0 ? IsNullValue : IsNotNullValue;
            }

            return value == null ? IsNullValue : IsNotNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
