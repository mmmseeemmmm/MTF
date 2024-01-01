using System.Linq;
using MTFApp.ReportViewer.ReportingWcf;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.ReportViewer
{
    class SequenceReportPreviews : MTFClientServerCommon.PagedList<SequenceReportPreview>
    {
        private ReportFilter filter;
        public SequenceReportPreviews(int pageSize, ReportFilter filter) : base(pageSize)
        {
            this.filter = filter.Clone() as ReportFilter;
            this.filter.PageSize = pageSize;

            SetCount(ReportingClient.GetReportingClient().GetReportsCount(filter).Result);
        }

        protected override void SaveData(int pageNumber, SequenceReportPreview[] data)
        {
        }

        protected override SequenceReportPreview[] LoadData(int pageNumber)
        {
            filter.PageNumber = pageNumber;
            return ReportingClient.GetReportingClient().GetReports(filter).Result.ToArray();
        }

        protected override void ClearSavedData()
        {
        }
    }
}
