using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MTFApp.UIHelpers.Converters
{
    public class MTFColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is AutomotiveLighting.MTFCommon.MTFColors)
            {
                if ((AutomotiveLighting.MTFCommon.MTFColors)value != AutomotiveLighting.MTFCommon.MTFColors.None)
                {
                    return App.Current.FindResource(value.ToString() + "Brush");
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
