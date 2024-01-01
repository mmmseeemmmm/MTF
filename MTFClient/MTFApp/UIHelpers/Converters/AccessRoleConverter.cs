using System;
using System.Globalization;
using System.Windows.Data;
using MTFClientServerCommon.MTFAccessControl;

namespace MTFApp.UIHelpers.Converters
{
    class AccessRoleConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return AccessRoleNameConvertor.ConvertAccessName(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
