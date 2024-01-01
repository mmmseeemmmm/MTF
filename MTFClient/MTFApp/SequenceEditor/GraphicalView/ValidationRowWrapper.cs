using System;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class ValidationRowWrapper : GraphicalViewTableItemBase
    {
        private MTFValidationTableRow row;
        private readonly Guid tableId;

        public ValidationRowWrapper(MTFValidationTableRow mtfValidationTableRow, Guid tableId)
        {
            row = mtfValidationTableRow;
            this.tableId = tableId;
        }

        public MTFValidationTableRow Row
        {
            get => row;
            set => row = value;
        }

        public override string Alias => Row?.Header;
        public override Guid TableId => tableId;
        public override Guid RowId => Row?.Id ?? Guid.Empty;
    }
}