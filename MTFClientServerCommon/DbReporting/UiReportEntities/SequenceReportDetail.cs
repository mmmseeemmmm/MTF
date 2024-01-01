using System;
using System.Collections.Generic;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportDetail : SequenceReportPreview
    {
        public string WinUser { get; set; }
        public string Machine { get; set; }
        public string GsRemains { get; set; }
        public bool GsWarning { get; set; }
        public bool ShowHiddenRows { get; set; }
        public string SequenceName { get; set; }
        public List<SequenceReportMessageDetail> Messages { get; set; }
        public List<SequenceReportErrorDetail> Errors { get; set; }
        public List<SequenceReportValidationTableDetail> ValidationTables { get; set; }
        public List<RoundingRuleUi> RoundingRules { get; set; }
        public string DisplayStatus
        {
            get
            {
                if (SequenceStatus.HasValue)
                {
                    return SequenceStatus.Value ? BaseConstants.ExecutionStatusOk : BaseConstants.ExecutionStatusNok;
                }

                return string.Empty;
            }
        }

    }
}