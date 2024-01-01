using System;
using System.Globalization;
using System.Windows.Data;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.UIHelpers.Converters
{
    public class ValidationTableAssignEvaluationConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var r = value as MTFValidationTableRow;
            if (r != null)
            {
                r.AssignEvaluatedVarianst();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
