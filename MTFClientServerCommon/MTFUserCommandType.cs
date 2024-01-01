using System.ComponentModel;

namespace MTFClientServerCommon
{
    public enum MTFUserCommandType
    {
        [Description("Button")]
        Button,
        [Description("Toggle Button")]
        ToggleButton,
        [Description("Indicator Red/Green")]
        IndicatorRedGreen,
        [Description("Indicator Gray/Green")]
        IndicatorGrayGreen,
        [Description("Indicator Gray/Red")]
        IndicatorGrayRed,
    }
}
