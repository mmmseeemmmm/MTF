using System.Globalization;
using System.Windows.Controls;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.ValidationRules
{
    class EmptyStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            return !string.IsNullOrEmpty(str)
                ? new ValidationResult(true, null)
                : new ValidationResult(false, LanguageHelper.GetString("ValidationRule_EmptyStringError"));
        }
    }
}