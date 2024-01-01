using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueType = value.GetType();
            if (!valueType.IsEnum)
            {
                return Visibility.Collapsed;
            }

            if (string.Compare(value.ToString(), VisibleEnumValue, true) == 0)
            {
                return Visibility.Visible;
            }

            if (VisibleEnumValue.Contains(","))
            {
                var visibleValues = VisibleEnumValue.Split(',');
                if (visibleValues.Any(v => v == value.ToString()))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string VisibleEnumValue
        {
            get;
            set;
        }
    }
}
