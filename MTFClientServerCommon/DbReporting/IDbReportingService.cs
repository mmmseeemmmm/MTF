using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFClientServerCommon.DbReporting
{
    [ServiceContract]
    public interface IDbReportingService
    {
        [OperationContract]
        string TestConnection();

        [OperationContract]
        Task<List<string>> GetSequenceNames();

        [OperationContract]
        Task<int> GetReportsCount(ReportFilter filter);

        [OperationContract]
        Task<IEnumerable<SequenceReportPreview>> GetReports(ReportFilter filter, ReportSorting sorting);

        [OperationContract]
        Task<IEnumerable<SummaryReportData>> GetReportsCountsPerTimeSlice(ReportFilter filter, int timeSliceInMinutes);

        [OperationContract]
        Task<IEnumerable<double>> GetTableSummaryData(ReportFilter filter, string tableName, string rowName, string columnName);

        [OperationContract]
        Task<SequenceReportDetail> GetDetail(int reportId);

        [OperationContract]
        Stream GetReportImageData(string path);

        [OperationContract]
        Stream GetGraphicalUIImageData(string path);

        [OperationContract]
        Stream GetPdfReport(int reportId);

        [OperationContract]
        Task<List<SummaryReportSettings>> GetSummaryReports();

        [OperationContract]
        Task<List<SummaryReportSettings>> SaveSummarySetting(List<SummaryReportSettings> data);

        [OperationContract]
        Task RemoveSummarySetting(List<SummaryReportSettings> data);
    }
}