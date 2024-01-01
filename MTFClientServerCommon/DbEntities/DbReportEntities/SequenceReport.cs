using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class SequenceReport : DbEntity, ICloneable
    {
        public SequenceReport()
        {
            StartTime = DateTime.Now;
            Messages = new List<ReportMessage>();
            Errors = new List<ReportError>();
            ValidationTables = new List<ReportValidationTable>();
        }

        public string SequenceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public bool? SequenceStatus { get; set; }
        public string GsRemains { get; set; }
        public bool GsWarning { get; set; }
        public string CycleName { get; set; }
        public bool ShowHiddenRows { get; set; }
        public string GraphicalViews { get; set; }
        public List<ReportMessage> Messages { get; set; }

        public List<ReportError> Errors { get; set; }
        public List<ReportValidationTable> ValidationTables { get; set; }

        public int? VariantVersionId { get; set; }

        [ForeignKey("VariantVersionId")]
        public ReportSequenceVariant VariantVersion { get; set; }
        public int? VariantLightDistributionId { get; set; }

        [ForeignKey("VariantLightDistributionId")]
        public virtual ReportSequenceVariant VariantLightDistribution { get; set; }

        public int? VariantMountingSideId { get; set; }

        [ForeignKey("VariantMountingSideId")]
        public ReportSequenceVariant VariantMountingSide { get; set; }
        public int? VariantGsDutId { get; set; }

        [ForeignKey("VariantGsDutId")]
        public ReportSequenceVariant VariantGsDut { get; set; }


        public int SequenceRunId { get; set; }

        [ForeignKey("SequenceRunId")]
        public ReportSequenceRun SequenceRun { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool IsEmpty => (ValidationTables == null || ValidationTables.Count < 1) && (Errors == null || Errors.Count < 1) &&
                               (Messages == null || Messages.Count < 1);
    }
}