using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class IsZeroToVisibilityConverter : IValueConverter
    {
        public IsZeroToVisibilityConverter()
        {
            IsZeroValue = Visibility.Collapsed;
            IsNotZeroValue = Visibility.Visible;
        }

        public Visibility IsZeroValue { get; set; }

        public Visibility IsNotZeroValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = string.Format(CultureInfo.InvariantCulture, "{0:0.000}", value);

            return s == "0.000" ? IsZeroValue : IsNotZeroValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
