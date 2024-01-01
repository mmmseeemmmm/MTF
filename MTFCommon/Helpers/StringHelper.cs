using System;
using System.Globalization;

namespace MTFCommon.Helpers
{
    public static class StringHelper
    {
        public static object ToNumeric(this string s)
        {
            int? i = s.ToInt();
            if (i != null)
            {
                return (int)i;
            }

            //if floating point - use decimal type - better precision
            return s.ToDecimal();
        }

        public static int? ToInt(this string s)
        {
            if (s.Contains(" "))
            {
                return null;
            }

            int i;
            if (int.TryParse(s, out i))
            {
                return i;
            }

            return null;
        }

        public static float ToFloat(this string s)
        {
            if (s.Contains(" "))
            {
                return float.NaN;
            }
            float f;
            if (float.TryParse(s.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out f))
            {
                return f;
            }

            return float.NaN;
        }

        public static double ToDouble(this string s)
        {
            if (s.Contains(" "))
            {
                return double.NaN;
            }
            double d;
            if (double.TryParse(s.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out d))
            {
                return d;
            }

            return double.NaN;
        }

        public static decimal? ToDecimal(this string s)
        {
            if (s.Contains(" "))
            {
                return null;
                //throw new Exception(string.Format("{0} isn't decimal number", s));
            }
            decimal d;
            if (decimal.TryParse(s.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out d))
            {
                return d;
            }

            return null;
            //throw new Exception(string.Format("{0} isn't decimal number", s));
        }
    }
}
