using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    public class SetupEnumToBrushConverter : IValueConverter
    {
        FrameworkElement element = new FrameworkElement();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((StateDebugSetup)value == StateDebugSetup.Active)
            {
                return element.FindResource("ALLightRedBrush");
            }
            return element.FindResource("ALLightSilverBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
