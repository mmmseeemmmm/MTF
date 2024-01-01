using System.Collections.Generic;

namespace MTFClientServerCommon
{
    public interface ISequenceClassInfo
    {
        MTFSequenceClassInfo ClassInfo { get; set; }
        IList<MTFParameterValue> MTFParameters { get; set; }
    }
}