using System;
using System.Globalization;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class TimeStampConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                double elapsedDouble;
                if (double.TryParse(value.ToString(), out elapsedDouble))
                {
                    if (elapsedDouble < 1000)
                    {
                        return string.Format("{0:0.###} ms", elapsedDouble);
                    }
                    if (elapsedDouble < (1000 * 60))
                    {
                        var diff = elapsedDouble / 1000;
                        return string.Format("{0:0.###} s", diff);
                    }
                    if (elapsedDouble < (1000 * 60 * 60))
                    {
                        var diff = elapsedDouble / (1000 * 60);
                        var minutes = Math.Truncate(diff);
                        var seconds = (diff - minutes) * 60;
                        return string.Format("00:{0:00}:{1:00.00}", minutes, seconds);
                    }
                    var diffHours = elapsedDouble / (1000 * 60 * 60);
                    var hours = Math.Truncate(diffHours);
                    var minutesDecimal = (diffHours - hours) * 60;
                    var m = Math.Truncate(minutesDecimal);
                    var s = (minutesDecimal - m) * 60;
                    return string.Format("{0:00}:{1:00}:{2:00.00}", hours, m, s);
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
