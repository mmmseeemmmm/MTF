using System;

namespace AutomotiveLighting.MTFCommon
{
    [Serializable]
    public class MTFActivity
    {
        public Guid Id { get; set; }
        public string ActivityName { get; set; }
        public string ClassAlias { get; set; }
        public string MethodName { get; set; }
    }
}
