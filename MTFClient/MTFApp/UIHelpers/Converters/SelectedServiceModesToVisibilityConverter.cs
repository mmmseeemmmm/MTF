using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    public class SelectedServiceModesToVisibilityConverter : IValueConverter
    {
        public MTFServiceModeVariants ModeVariant { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var usedVariant = value as IList<MTFServiceModeVariants>;

            return usedVariant != null && usedVariant.Contains(ModeVariant) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
