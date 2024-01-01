using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class VariableByTypeGroupConverter : IMultiValueConverter
    {
        private string[] numberTypes = 
        { 
                typeof(sbyte).FullName,
                typeof(byte).FullName,
                typeof(short).FullName,
                typeof(ushort).FullName,
                typeof(int).FullName,
                typeof(uint).FullName,
                typeof(long).FullName,
                typeof(ulong).FullName,
                typeof(float).FullName,
                typeof(double).FullName
        };

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<MTFVariable> variables = values[0] as IEnumerable<MTFVariable>;
            TermGroups termGroup = (TermGroups)values[1];
            if (variables != null)
            {
                return variables.Where(x => IsTypeNameInTargetGroup(x.TypeName, termGroup));
            }
            return null;
        }

        private bool IsTypeNameInTargetGroup(string typeName, TermGroups termGroup)
        {
            if (termGroup == TermGroups.None)
            {
                return true;
            }
            return ((termGroup & TermGroups.NumberTerm) == TermGroups.NumberTerm && numberTypes.Contains(typeName))
                ||
                ((termGroup & TermGroups.LogicalTerm) == TermGroups.LogicalTerm && typeName == typeof(bool).FullName)
                ||
                ((termGroup & TermGroups.StringTerm) == TermGroups.StringTerm && typeName == typeof(string).FullName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
