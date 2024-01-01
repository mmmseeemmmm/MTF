using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class ValidationTableCellStatusConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == System.Windows.DependencyProperty.UnsetValue || values[1] == System.Windows.DependencyProperty.UnsetValue)
            {
                return null;
            }
            var rowStatus = (MTFCommon.MTFValidationTableStatus)values[0];
            var cellStatus = (bool)values[1];
            return rowStatus == MTFCommon.MTFValidationTableStatus.NotFilled || cellStatus;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
