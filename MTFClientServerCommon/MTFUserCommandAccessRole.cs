using System.ComponentModel;

namespace MTFClientServerCommon
{
    public enum MTFUserCommandAccessRole
    {
        [Description("Every one")]
        EveryOne,
        [Description("Logged user")]
        LoggedUser,
        [Description("Service role")]
        ServiceRole,
    }
}
