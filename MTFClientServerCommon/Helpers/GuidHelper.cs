using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.Helpers
{
    public static class GuidHelper
    {
        public static bool CompareGuidPath(IList<Guid> guidPath1, IList<Guid> guidPath2)
        {
            if (guidPath1.Count != guidPath2.Count)
            {
                return false;
            }
            for (int i = 0; i < guidPath1.Count; i++)
            {
                if (guidPath1[i] != guidPath2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
