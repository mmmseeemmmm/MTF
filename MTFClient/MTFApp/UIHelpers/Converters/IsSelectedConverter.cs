using System;
using System.Collections;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class IsSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var list = values[1] as IList;
            if (list != null && list.Count > 0)
            {
                return list.Contains(values[0]);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
