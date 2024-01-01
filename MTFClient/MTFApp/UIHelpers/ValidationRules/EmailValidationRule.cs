using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.ValidationRules
{
    class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    bool isEmail = Regex.IsMatch(str,
                        @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                        RegexOptions.IgnoreCase);
                    return isEmail
                        ? new ValidationResult(true, null)
                        : new ValidationResult(false, LanguageHelper.GetString("ValidationRule_WrongEmailAddress"));
                }
                catch (Exception)
                {
                    return new ValidationResult(false, LanguageHelper.GetString("ValidationRule_WrongEmailAddress"));
                }
            }
            return new ValidationResult(false, LanguageHelper.GetString("ValidationRule_EmptyStringError"));
        }
    }
}