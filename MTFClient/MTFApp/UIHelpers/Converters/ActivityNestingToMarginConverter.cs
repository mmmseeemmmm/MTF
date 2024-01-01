using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    class ActivityNestingToMarginConverter: IValueConverter
    {
        public int DefaultMargin { get; set; }

        public ActivityNestingToMarginConverter()
        {
            DefaultMargin = 20;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int nesting;
            if (value!=null && int.TryParse(value.ToString(), out nesting))
            {
                return string.Format("{0} 0 0 0", DefaultMargin * nesting);
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
