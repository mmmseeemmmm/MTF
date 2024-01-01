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
    public class VariableByTypeConverter : IMultiValueConverter
    {
        public string DefaultVariableTypeName { get; set; }

        public VariableByTypeConverter()
        {
            DefaultVariableTypeName = typeof(string).FullName;
        }


        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == null || values[0] == System.Windows.DependencyProperty.UnsetValue)
            {
                return null;
            }

            Type variableType = values.Length > 1 ? Type.GetType(values[0].ToString()) : Type.GetType(DefaultVariableTypeName);
            IList<MTFVariable> variables = values.Length > 1 ? values[1] as IList<MTFVariable> : values[0] as IList<MTFVariable>;

            if (variableType == null)
            {
                return null;
            }

            if (variables != null)
            {
                return variables.Where(x => x.TypeName == variableType.FullName);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
