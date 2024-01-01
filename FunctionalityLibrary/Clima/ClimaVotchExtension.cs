using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClimaVotchExtension
{
    public static class ClimaVotchExtension
    {
        public static double GetDouble(this string inputstring)
        {

            Match m = Regex.Match(inputstring, "[+-]?[0-9]+[.[0-9]+]?");
            inputstring = m.Value.TrimStart('0');
            //s=s.Replace('.', ',');
            return Convert.ToDouble(inputstring, CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public static string FormatIntoString(this double inputdouble)
        {
            string s = inputdouble.ToString("#0.00");
            s = s.Replace(",", ".");
            return s;
        }

        public static double CheckLimits(this double input, double lowLimit, double highLimit)
        {
            if ((input >= lowLimit) && (input <= highLimit))
            {
                return input;
            }
            else
            {
                throw new Exception("Set Value exceeded Limit");
            }
        }
              
        

        public static void FormatIntoString(this string inputstring)
        {

        }

        

    }
}
