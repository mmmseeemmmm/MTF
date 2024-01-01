using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MTFClientServerCommon;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;

namespace MTFApp.UIHelpers.Converters
{
    class ActivityStatusImageConverter : IValueConverter
    {
        private static Style okImage = (Style)Application.Current.FindResource("IconOk");
        private static Style nokImage = (Style)Application.Current.FindResource("IconNok");
        private static Style warningImage = (Style)Application.Current.FindResource("IconWarning");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MTFExecutionActivityStatus)
            {
                return GetImageFromStatus((MTFExecutionActivityStatus)value);
            }
            return null;
        }

        private object GetImageFromStatus(MTFExecutionActivityStatus executionActivityStatus)
        {
            switch (executionActivityStatus)
            {
                case MTFExecutionActivityStatus.None:
                    return null;
                case MTFExecutionActivityStatus.Ok:
                    return okImage;
                case MTFExecutionActivityStatus.Nok:
                    return nokImage;
                case MTFExecutionActivityStatus.Warning:
                    return warningImage;
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
