using System;
using System.Collections.Generic;

namespace AutomotiveLighting.MTFCommon
{
    [Serializable]
    public class MTFDataTable
    {
        private bool canSort;

        public bool CanSort
        {
            get { return canSort; }
            set { canSort = value; }
        }
        private bool canReorderColumns;

        public bool CanReorderColumns
        {
            get { return canReorderColumns; }
            set { canReorderColumns = value; }
        }
        private bool canResizeColumns;

        public bool CanResizeColumns
        {
            get { return canResizeColumns; }
            set { canResizeColumns = value; }
        }
        private bool canAddRow;

        public bool CanAddRow
        {
            get { return canAddRow; }
            set { canAddRow = value; }
        }
        private HeaderVisibility headerVisibility;

        public HeaderVisibility HeaderVisibility
        {
            get { return headerVisibility; }
            set { headerVisibility = value; }
        }
        private List<ColumnDescription> columns;

        public List<ColumnDescription> Columns
        {
            get { return columns; }
            set { columns = value; }
        }
        private List<List<object>> tableData;

        public List<List<object>> TableData
        {
            get { return tableData; }
            set { tableData = value; }
        }
    }

    public enum HeaderVisibility
    { 
        No,
        Column,
        Row,
        All
    }
}
