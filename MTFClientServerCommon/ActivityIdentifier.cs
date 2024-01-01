using System;

namespace MTFClientServerCommon
{
    [Serializable]
    public class ActivityIdentifier
    {
        public string ActivityKey { get; set; }
        public int UniqueIndexer { get; set; }
    }
}
