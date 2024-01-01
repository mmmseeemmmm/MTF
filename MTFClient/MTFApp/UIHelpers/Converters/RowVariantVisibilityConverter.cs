using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.UIHelpers.Converters
{
    class RowVariantVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cell = value as MTFValidationTableCell;
            if (cell != null && (
                cell.Type == MTFTableColumnType.ActualValue
                || cell.Type == MTFTableColumnType.Identification 
                || cell.Type == MTFTableColumnType.GoldSample
                || cell.Type == MTFTableColumnType.Hidden))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
