using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportValidationTable : DbEntity
    {
        public string Name { get; set; }

        public DbReportTableValidationMode ValidationMode { get; set; }

        public DbReportValidationTableStatus TableStatus { get; set; }

        public DateTime ValidationTime { get; set; }

        public List<ReportValidationTableRow> Rows { get; set; }

        public int SequenceReportId { get; set; }
        [ForeignKey("SequenceReportId")]
        public SequenceReport SequenceReport { get; set; }
    }
}