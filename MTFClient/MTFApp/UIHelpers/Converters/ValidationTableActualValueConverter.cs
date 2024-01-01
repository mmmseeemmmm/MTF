using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    public class ValidationTableActualValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                var value = values[0];
                var items = value as ICollection;
                if (items != null && items.Count > 0)
                {
                    StringBuilder outuput = new StringBuilder();
                    foreach (var item in items)
                    {
                        outuput.Append(item).Append(", ");
                    }
                    return outuput.ToString().Substring(0, outuput.Length - 2);
                }
                if (value != DependencyProperty.UnsetValue)
                {
                    var roundRules = values[1] as IList<RoundingRule>;
                    var result = MTFSequenceHelper.RoundActualValue(value, roundRules);
                    return result != null ? result.ToString() : null;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
