using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.UIHelpers
{
    public static class RoundingHelper
    {
        public static object RoundStringValue(string value, IList<RoundingRuleUi> roundingRules)
        {
            if (value == null)
            {
                return null;
            }

            if (double.TryParse(value, out var d))
            {
                var absValue = Math.Abs(d);
                var rule = roundingRules.FirstOrDefault(r => absValue >= r.Min && absValue <= r.Max);
                if (rule != null)
                {
                    var formatter = new StringBuilder("0.").Append(new string('0', rule.Digits));
                    return Math.Round(d, rule.Digits).ToString(formatter.ToString());
                }
            }
            return value;
        }
    }
}
