using System;
using System.Collections;
using System.Text;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class ValidationTableValueToStringConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var items = value as ICollection;
            if (items != null && items.Count>0)
            {
                StringBuilder outuput = new StringBuilder();
                foreach (var item in items)
                {
                    outuput.Append(item).Append(", ");
                }
                return outuput.ToString().Substring(0, outuput.Length - 2);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
