using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    public class StatusMessageTypeToIconConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();

            if (value is StatusMessage.MessageType)
            {
                switch ((StatusMessage.MessageType)value)
                {
                    case StatusMessage.MessageType.Error:
                        return element.FindResource("IconNok");
                    case StatusMessage.MessageType.Warning:
                        return element.FindResource("IconWarning");
                    case StatusMessage.MessageType.Info:
                        return element.FindResource("IconInfo");
                    default: return element.FindResource("IconNone");
                }
            }
            return element.FindResource("IconNone");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
