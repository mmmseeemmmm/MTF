using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    class ControlStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();

            if (!(value is ControlStatus))
            {
                return DefaultColorIsWhite ? element.FindResource("ALWhiteBrush") : element.FindResource("ALYellowBrush");
            }

            switch ((ControlStatus)value)
            {
                    case ControlStatus.Error: return element.FindResource("ALLightRedBrush");
                    case ControlStatus.Warning: return element.FindResource("ALColor8Brush");
                    case ControlStatus.GoldSample: return element.FindResource("ALColor4Brush");

                    default: return DefaultColorIsWhite ? element.FindResource("ALWhiteBrush") : element.FindResource("ALYellowBrush"); 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public bool DefaultColorIsWhite { get; set; }
    }
}
