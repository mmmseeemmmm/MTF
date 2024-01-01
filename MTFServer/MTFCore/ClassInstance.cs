using MTFClientServerCommon;
using System;

namespace MTFCore
{
    class ClassInstance
    {
        public MTFClassInfo ClassInfo { get; set; }
        public Guid ClassInstanceConfigurationId { get; set; }
        public object Instance { get; set; }
        public bool IsEnabled { get; set; }
    }
}
