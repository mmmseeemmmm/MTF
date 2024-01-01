using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFCommon.Helpers
{
    public static class TypeHelper
    {
        private static Type[] numericTypes = { 
                typeof(sbyte), typeof(byte),
                typeof(short), typeof(ushort),
                typeof(int) ,typeof(uint),
                typeof(long), typeof(ulong),
                typeof(float), typeof(double),
                typeof(decimal)
            };

        private static Dictionary<Type, Type[]> typeCanBeAssignedTo = new Dictionary<Type, Type[]> {
            {typeof(sbyte), new[] { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(byte), new[] { typeof(byte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(short), new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(ushort), new[] { typeof(ushort), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(int), new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(uint), new[] { typeof(uint), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(long), new[] { typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(ulong), new[] { typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(float), new[] { typeof(float), typeof(double), typeof(decimal), typeof(object) }},
            {typeof(double), new[] { typeof(double), typeof(decimal), typeof(object) }},
            {typeof(decimal), new[] { typeof(decimal), typeof(object) }},
            {typeof(char), new[]{typeof(char), typeof(string), typeof(object)}},
            {typeof(string), new[]{typeof(string), typeof(object)}},
            {typeof(bool), new[]{typeof(bool), typeof(object)}},
            {typeof(object), new[]{typeof(object)}},
        };


        public static bool IsNumericType(this Type type)
        {
            return numericTypes.Contains(type);
        }

        public static Type GetCommonType(Type type1, Type type2)
        {
            //if (typeCanBeAssignedTo[type2].Contains(type1))
            //{
            //    return type1;
            //}

            //if (typeCanBeAssignedTo[type1].Contains(type2))
            //{
            //    return type2;
            //}

            try
            {
                return typeCanBeAssignedTo[type1].Intersect(typeCanBeAssignedTo[type2]).First();
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Can't compare values of type {0} and {1}", type1, type2));
            }
        }

        public static object ConvertValue(object value, string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                return null;
            }

            return ConvertValue(value, type);
        }

        public static object ConvertValue(object value, Type toType)
        {
            if (value == null)
            {
                return null;
            }

            Type fromType = value.GetType();
            //if property is in given type
            if (toType == fromType)
            {
                return value;
            }

            if (fromType == typeof(string))
            {
                if (toType.IsNumericType())
                {
                    string s = (string)value;

                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return null;
                    }

                    s = s.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    s = s.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    return Convert.ChangeType(s, toType);
                }
            }

            if (value is IConvertible)
            {
                try
                {
                    return Convert.ChangeType(value, toType);
                }
                catch
                {
                    return null;
                }
            }
            return value;
        }
    }
}
