using System;
using System.Windows.Data;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Converters
{
    public class EnumToDescriptionLocalizedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = (Enum)value;
            return item != null ? LanguageHelper.GetString(item.Description()) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
