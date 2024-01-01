using System;

namespace MTFCommon
{
    [Serializable]
    public class MTFSequenceVariantValue
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
