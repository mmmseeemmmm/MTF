using System.ComponentModel;

namespace MTFClientServerCommon.MTFValidationTable
{

    public enum ValidationTableResultType
    {
        [Description("Table result")]
        TableResult,
        [Description("Line result")]
        LineResult,
        [Description("Cell content")]
        CellResult,
    }
}
