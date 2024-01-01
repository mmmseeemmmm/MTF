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
    class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();

            if (!(value is StatusMessage.MessageType))
            {
                return element.FindResource("ALYellowBrush");
            }

            switch ((StatusMessage.MessageType)value)
            {
                case StatusMessage.MessageType.Error: return element.FindResource("ALRedBrush");
                case StatusMessage.MessageType.Warning: return element.FindResource("ALYellowBrush");

                default: return element.FindResource("ALBlackBrush");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
