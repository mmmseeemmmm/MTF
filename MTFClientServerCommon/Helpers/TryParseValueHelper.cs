using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Helpers
{
    public static class TryParseValueHelper
    {
        /// <summary>
        /// Try parse value as definied type
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="valueType">Value Type</param>
        /// <returns>Message if is parse error.</returns>
        public static string TryParseToString(object value, Type valueType)
        {
            var tryParseMethodInfo = GetTryParseMethod(valueType);
            if (tryParseMethodInfo != null)
            {
                object[] args = { value as string, null };
                if (!(bool)tryParseMethodInfo.Invoke(null, args))
                {
                    return "This value is not valid for " + valueType.Name;
                }
                return string.Empty;
            }
            return "Cannot validate this value for " + valueType.Name;
        }

        public static bool TryParse(object value, Type valueType)
        {
            var tryParseMethodInfo = GetTryParseMethod(valueType);
            if (tryParseMethodInfo != null)
            {
                value = value != null ? value.ToString() : null;
                object[] args = { value, null };
                if ((bool)tryParseMethodInfo.Invoke(null, args))
                {
                    return true;
                }
            }
            //else if (valueType== typeof(string))
            //{
            //    return true;    
            //}
            return false;
        }

        private static System.Reflection.MethodInfo GetTryParseMethod(Type type)
        {
            if (type == null)
            {
                return null;
            }
            Type[] argTypes = { typeof(string), type.MakeByRefType() };
            return type.GetMethod("TryParse", argTypes);
        }
    }
}
