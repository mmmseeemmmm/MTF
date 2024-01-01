using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class MTFAdditionalParameterInfoAttribute : Attribute
    {
        public string ParameterName { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string SetupMethodName { get; set; }
        public string DefaultValue { get; set; }
        public string ValueListName { get; set; }
        public int ValueListLevel { get; set; }
        public string ValueListParentName { get; set; }
        public string DataNameExtension { get; set; } //Pavel Fexa knows what this means ;)
        public bool IsOptional { get; set; }
        public string MaxValue { get; set; }
        public string MinValue { get; set; }
    }
}
