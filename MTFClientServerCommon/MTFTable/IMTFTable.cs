using System;
using System.Collections.Generic;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon.MTFTable
{
    public interface IMTFTable
    {
        string Name { get; set; }
        bool SaveToLog { get; set; }
        int RowsCount { get; }
        int ColumnsCount { get; }
        IEnumerable<string> ColumnNames { get; }


        object GetResult(ValidationTableResultType selectedResultType, Guid selectedRowId, string selectedColumn, SequenceVariant sequenceVariant);
        //void ChangeVariant(IList<SequenceVariantGroup> sequenceVariantGroups, IEnumerable<SequenceVariantValue> newValues);
        bool ExistRow(Guid rowId);
        string GetRowNameById(Guid rowId);
        Guid GetRowIdByName(string name);
    }
}
