using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.GoldSamplePersist
{
    [Serializable]
    public class GoldSampleValue
    {
        private object defaultValue;
        private Dictionary<Guid, object> variantValues;


        public object DefaultValue
        {
            get { return defaultValue; }
            set { this.defaultValue = value; }
        }

        public Dictionary<Guid, object> VariantValues
        {
            get { return variantValues; }
            set { variantValues = value; }
        }
    }
}
