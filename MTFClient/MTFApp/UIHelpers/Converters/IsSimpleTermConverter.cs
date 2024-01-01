using MTFClientServerCommon.Mathematics;
using System;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class IsSimpleTermConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value is Term) || value is ConstantTerm || value is EmptyTerm || value is ActivityResultTerm || value is TermWrapper || value is ActivityTargetTerm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
