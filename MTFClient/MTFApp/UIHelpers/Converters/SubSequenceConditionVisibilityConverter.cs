using MTFClientServerCommon;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class SubSequenceConditionVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is ExecutionType && values[1] is bool)
            {
                var executionType = (ExecutionType)values[0];
                var variantSwitch = (bool)values[1];
                if (executionType == ExecutionType.ExecuteAlways ||
                    executionType == ExecutionType.ExecuteByCall ||
                    executionType == ExecutionType.ExecuteInParallel ||
                    executionType == ExecutionType.ExecuteOnBackground ||
                    (executionType == ExecutionType.ExecuteByCase && variantSwitch))
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
