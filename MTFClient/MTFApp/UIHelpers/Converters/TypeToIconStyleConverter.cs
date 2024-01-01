using MTFClientServerCommon.Helpers;
using System;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class TypeToIconStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();
            TypeInfo ti = new TypeInfo(value.ToString());

            if (ti.IsBool)
            {
                return element.FindResource("IconTypeBool");
            }

            if (ti.IsNumeric)
            {
                return element.FindResource("IconTypeNumber");
            }

            if (ti.IsString)
            {
                return element.FindResource("IconTypeString");
            }

            if (ti.IsConstantTable)
            {
                return element.FindResource("IconTypeTable");
            }

            if (ti.IsValidationTable)
            {
                return element.FindResource("IconValidationTable");
            }

            return element.FindResource("IconNone");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
