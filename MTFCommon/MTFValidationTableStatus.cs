using System.ComponentModel;

namespace MTFCommon
{
    public enum MTFValidationTableStatus
    {
        [Description("Not filled")]
        NotFilled,
        [Description("OK")]
        Ok,
        [Description("NOK")]
        Nok,
        [Description("GS Fail")]
        GSFail,
    }
}
