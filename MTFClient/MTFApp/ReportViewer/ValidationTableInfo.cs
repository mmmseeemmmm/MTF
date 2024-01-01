using System.Collections.Generic;

namespace MTFApp.ReportViewer
{
    public class ValidationTableInfo
    {
        public string Name { get; set; }
        public IEnumerable<ValidationTableRowInfo> Rows { get; set; }
    }

    public class ValidationTableRowInfo
    {
        public string Name { get; set; }
        public IEnumerable<ValidationTableColumnInfo> Columns { get; set; }
    }

    public class ValidationTableColumnInfo
    {
        public string Name { get; set; }
    }
}
