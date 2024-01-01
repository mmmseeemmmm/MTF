using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MTFCommon
{
    [Serializable]
    public class MTFSequenceVariant
    {
        private IList<MTFSequenceVariantGroup> variantGroups = new List<MTFSequenceVariantGroup>();

        public IList<MTFSequenceVariantGroup> VariantGroups
        {
            get { return variantGroups; }
            set { variantGroups = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var group in VariantGroups)
            {
                if (group.Values != null && group.Values.Any())
                {
                    sb.Append("[").Append(string.Join(";", group.Values.Select(g => g.Name))).Append("] ");
                }
            }

            return sb.ToString();
        }
    }
}
