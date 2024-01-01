using System.Collections.ObjectModel;

namespace MTFClientServerCommon.MTFTable
{
    public interface IMTFDataTable
    {
        ObservableCollection<MTFTableRow> Rows { get; set; }
        ObservableCollection<MTFTableColumn> Columns { get; set; }
        void AddRow();
        void AddColumn();
        void RemoveRow(MTFTableRow row);
        void RemoveColumn(MTFTableColumn column);
        void MoveUp(MTFTableRow row);
        void MoveDown(MTFTableRow row);
    }
}
