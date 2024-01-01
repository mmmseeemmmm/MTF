using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.UIHelpers.Converters
{
    class TableReportActualValueRoundingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 1 && values[0] is string actualValue && !string.IsNullOrEmpty(actualValue))
            {
                if (values[1] is IList<RoundingRuleUi> rules && rules.Any())
                {
                    return RoundingHelper.RoundStringValue(actualValue, rules);
                }

                return actualValue;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}