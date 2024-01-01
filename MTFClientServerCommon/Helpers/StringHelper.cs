using System;
using System.Globalization;
using System.Text;

namespace MTFClientServerCommon.Helpers
{
    public static class StringHelper
    {
        public static string GetNonPunctionalLowerString(this string s)
        {
            if (s == null)
            {
                return null;
            }

            string stringFormD = s.ToLower().Normalize(NormalizationForm.FormD);

            var retVal = new StringBuilder();
            foreach (char ch in stringFormD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    retVal.Append(ch);
                }
            }
            return retVal.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GetExceptionStringMessage(Exception ex)
        {
            return GetExceptionStringMessage(ex, false);
        }
        public static string GetExceptionStringMessage(Exception ex, bool newLineForEachMsg)
        {
            var sb = new StringBuilder();
            FillExceptionStringMessage(ex, sb, newLineForEachMsg);
            return sb.ToString();
        }

        private static void FillExceptionStringMessage(Exception ex, StringBuilder sb, bool newLineForEachMsg)
        {
            sb.Append(ex.Message);
            if (ex.InnerException!=null)
            {
                if (newLineForEachMsg)
                {
                    sb.AppendLine();
                }

                FillExceptionStringMessage(ex.InnerException, sb, newLineForEachMsg);
            }

        }

        
    }
}
