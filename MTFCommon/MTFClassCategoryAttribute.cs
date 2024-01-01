using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class MTFClassCategoryAttribute : Attribute
    {
        public MTFClassCategoryAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
