using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class MTFAllowedParameterValueAttribute : Attribute
    {
        public MTFAllowedParameterValueAttribute(string parameterName, string parameterValueDisplayText, object parameterValue)
        {
            ParameterName = parameterName;
            ParameterValueDisplayText = parameterValueDisplayText;
            ParameterValue = parameterValue;
        }
        public string ParameterName { get; set; }
        public string ParameterValueDisplayText { get; set; }
        public object ParameterValue { get; set; }
    }
}
