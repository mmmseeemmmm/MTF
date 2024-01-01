using System;
using System.ComponentModel.DataAnnotations.Schema;
using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportValidationTableRow : DbEntity
    {
        public string Name { get; set; }
        public string ActualValue { get; set; }
        public DbReportValidationTableStatus Status { get; set; }
        public int NumberOfRepetition { get; set; }
        public bool HasImage { get; set; }
        public bool IsHidden { get; set; }
        public string MinValue { get; set; }
        public bool MinStatus { get; set; }
        public string MaxValue { get; set; }
        public bool MaxStatus { get; set; }
        public string RequiredValue { get; set; }
        public bool RequiredStatus { get; set; }
        public string ProhibitedValue { get; set; }
        public bool ProhibitedStatus { get; set; }
        public string GsValue { get; set; }
        public bool GsStatus { get; set; }
        public double GsPercentage { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ValidationTableId { get; set; }
        [ForeignKey("ValidationTableId")]
        public ReportValidationTable ValidationTable { get; set; }
    }
}