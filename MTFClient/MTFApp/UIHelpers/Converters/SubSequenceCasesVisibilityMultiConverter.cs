using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Converters
{
    class SubSequenceCasesVisibilityMultiConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 3 && values[0] is ExecutionType && values[1] is bool)
            {
                var executionType = (ExecutionType)values[0];
                var variantSwitch = (bool)values[1];
                if (executionType == ExecutionType.ExecuteByCase && (variantSwitch || (values[2] is Term && !(values[2] is EmptyTerm)) ))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
