using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using MTFCore.DbReporting.Repositories;
using MTFCore.ReportBuilder;

namespace MTFCore.DbReporting.DbReportingWcf
{
    class DbReportingCore : IDbReportingService, IDisposable
    {
        private SequenceReportRepository ReportRepository { get; }
        private SummaryReportRepository SummaryReportRepository { get; }

        public DbReportingCore()
        {
            ReportRepository = new SequenceReportRepository();
            SummaryReportRepository = new SummaryReportRepository();
        }
        public void Dispose()
        {
        }

        public string TestConnection()
        {
            return "OK";
        }

        public Task<List<string>> GetSequenceNames()
        {
            return Task.Run(() => ReportRepository.GetSequenceNames());
        }

        public Task<int> GetReportsCount(ReportFilter filter)
        {
            return Task.Run(() => ReportRepository.GetReportsCount(filter));
        }

        public Task<IEnumerable<SequenceReportPreview>> GetReports(ReportFilter filter, ReportSorting sorting)
        {
            return Task.Run(() => ReportRepository.GetReports(filter));
        }

        public Task<IEnumerable<SummaryReportData>> GetReportsCountsPerTimeSlice(ReportFilter filter, int timeSliceInMinutes)
        {
            return Task.Run(() => ReportRepository.GetReportsCountsPerTimeSlice(filter, timeSliceInMinutes));
        }

        public Task<IEnumerable<double>> GetTableSummaryData(ReportFilter filter, string tableName, string rowName, string columnName)
        {
            return Task.Run(() => ReportRepository.GetTableSummaryData(filter, tableName, rowName, columnName));
        }

        public Task<SequenceReportDetail> GetDetail(int reportId)
        {
            return Task.Run(() => ReportRepository.GetDetail(reportId));
        }

        public Stream GetReportImageData(string path) => 
            new FileStream(Path.Combine(BaseConstants.DataPath, BaseConstants.ReportImageBasePath, path), FileMode.Open);

        public Stream GetGraphicalUIImageData(string path) => 
            new FileStream(Path.Combine(BaseConstants.DataPath, BaseConstants.ReportGraphicalViewBasePath, path), FileMode.Open);

        public Stream GetPdfReport(int reportId) => new PdfReportDetailBuilder(ReportRepository.GetDetail(reportId)).Build();
        public Task<List<SummaryReportSettings>> GetSummaryReports()
        {
            return Task.Run(() => SummaryReportRepository.GetSummaryReports());
        }

        public Task<List<SummaryReportSettings>> SaveSummarySetting(List<SummaryReportSettings> data)
        {
            return Task.Run(()=>SummaryReportRepository.SaveSummarySetting(data));
        }

        public Task RemoveSummarySetting(List<SummaryReportSettings> data)
        {
            return Task.Run(() => SummaryReportRepository.RemoveSummarySetting(data));
        }
    }
}
