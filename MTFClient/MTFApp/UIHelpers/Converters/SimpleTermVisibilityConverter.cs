using MTFClientServerCommon.Mathematics;
using System;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class SimpleTermVisibilityConverter: IValueConverter
    {
        public Visibility IsSimpleTerm { get; set; }
        public Visibility IsNotSimpleTerm { get; set; }

        public SimpleTermVisibilityConverter()
        {
            IsSimpleTerm = Visibility.Collapsed;
            IsNotSimpleTerm = Visibility.Visible;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value is Term) || value is ConstantTerm || value is EmptyTerm || value is ActivityResultTerm || value is TermWrapper ||
                   value is ActivityTargetTerm
                ? IsSimpleTerm
                : IsNotSimpleTerm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
