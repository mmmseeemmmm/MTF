using System;
using System.Collections.Generic;

namespace MTFCommon
{
    [Serializable]
    public class MTFSequenceVariantGroup
    {
        private IList<MTFSequenceVariantValue> values = new List<MTFSequenceVariantValue>();
        private string name;

        public IList<MTFSequenceVariantValue> Values
        {
            get { return values; }
            set { values = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
