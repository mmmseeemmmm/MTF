using System.ComponentModel;

namespace MTFClientServerCommon
{
    public enum SequenceExecutionViewType
    {
        [Description("Enum_ExecutionType_Tree")]
        TreeView,
        [Description("Enum_ExecutionType_Table")]
        TableView,
        [Description("Enum_ExecutionType_Time")]
        TimeView,
        [Description("Enum_ExecutionType_Service")]
        Service,
        [Description("Enum_ExecutionType_Client")]
        ClientUi,
        [Description("Enum_ExecutionType_Graphical")]
        GraphicalView
    }
}
