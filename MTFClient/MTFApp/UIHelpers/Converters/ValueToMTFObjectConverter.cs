using MTFClientServerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class ValueToMTFObjectConverter : IValueConverter
    {
        //public static string TypeName { get; set; }
        //public static bool IsNullValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (parameter.ToString())
            {
                case "constructors":
                    return ConvertToString(value);
                case "properties":
                    return ConvertToList(value);
                default:
                    return null;
            }
        }

        private object ConvertToString(object value)
        {
            bool parameterLessConstructor = false;
            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    if (item is GenericConstructorInfo && (item as GenericConstructorInfo).Parameters.Count==0)
                    {
                        parameterLessConstructor = true;
                    }
                }
                return parameterLessConstructor ? null : "No parameterless constructor defined for this object.";
            }
            return null;
        }

        private object ConvertToList(object value)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
