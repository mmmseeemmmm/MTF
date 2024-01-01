﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class MillisecondsToHumanReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
            {
                return null;
            }

            TimeSpan ts = TimeSpan.FromMilliseconds((double)value);
            return string.Format($"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
