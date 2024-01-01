using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.UIHelpers.Converters
{
    class TableReportCriteriaRoundingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 1 && values[0] is ReportValidationTableColumnDetail column)
            {
                if (column.CanRound && values[1] is IList<RoundingRuleUi> rules && rules.Any())
                {
                    return RoundingHelper.RoundStringValue(column.Value, rules);
                }

                return column.Value;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}