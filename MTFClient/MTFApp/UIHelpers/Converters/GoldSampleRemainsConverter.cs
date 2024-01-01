using System;
using System.Globalization;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class GoldSampleRemainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double count;
            if (value != null && double.TryParse(value.ToString(), out count))
            {
                var h = (int)count / 60;
                var m = (int)count % 60;
                return string.Format("{2}{0}h{1:D2}m", h, Math.Abs(m), m < 0 || h < 0 ? "-" : string.Empty);
            }
            return "0m";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
