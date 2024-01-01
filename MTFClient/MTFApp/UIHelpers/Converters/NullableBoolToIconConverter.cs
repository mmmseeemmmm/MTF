using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class NullableBoolToIconConverter : IValueConverter
    {
        private static readonly Style okImage = (Style)Application.Current.FindResource("IconOk");
        private static readonly Style nokImage = (Style)Application.Current.FindResource("IconNok");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                switch (b)
                {
                    case true:
                        return okImage;
                    default:
                        return nokImage;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
