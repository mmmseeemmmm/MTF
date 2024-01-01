using System;
using System.Windows.Data;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = (Enum)value;
            return item != null ? item.Description() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
