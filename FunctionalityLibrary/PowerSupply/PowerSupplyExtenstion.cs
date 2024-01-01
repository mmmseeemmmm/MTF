using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PowerSupplyExtensionMethods//PowerSupply
{
    public static class PowerSupplyExtenstion
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
            s=s.Replace(",", ".");
            return s;
        }




        public static string CheckLambdaErrorOccured(this string error)
        {
            if (!error.Contains("No error"))
            {
                return error;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string CheckSyskonErrorOccured(this string error)
        {
            //SyskonError res; //= new SyskonError();
            //res.errorOcured = false;
            //res.errors = string.Empty;
            
            string[] errors = Regex.Split(error, @"\D+");
            List<string> errorCodes=errors.ToList<string>();
            errorCodes.RemoveRange(errorCodes.Count - 1, 1);
            string errs = string.Empty;
            foreach (string value in errorCodes)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    
                    int i=int.Parse(value);
                    if (i != 0)
                    {
                        if (string.IsNullOrEmpty(errs))
                        {
                            errs ="Error Codes:" + value;
                        }
                        else
                        {
                            errs = errs + "," + value;
                        }
                    }
                    
                }
            }
            return errs;
        }

        public static double GetPower(this string inputstring)
        {
            double power = 0;
            if (inputstring.Length > 20)
            {
                inputstring = inputstring.Substring(0, 20); //remove extra characters
            }
            string[] numbers = Regex.Split(inputstring, @"\D+");
            foreach (string value in numbers)
            {
                if (!string.IsNullOrEmpty(value))
                {

                    int i = int.Parse(value);
                    if (power == 0)
                    {
                        power = i;
                    }
                    else
                    {
                        power = power * i;
                    }
                    
                }
            }
            return power;
        }

        public static void FormatIntoString(this string inputstring)
        {

        }

        public static bool ErrorFromRampOccured(this string inputstring)
        {
            string[] errors = Regex.Split(inputstring, @"\r");
            foreach (string value in errors)
            {
                if (value != "OK" && value != "")
                {

                    return true;

                }
            }
            return false;
        }
        
    }
}
