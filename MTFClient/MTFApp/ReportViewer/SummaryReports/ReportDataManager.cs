using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.ReportViewer.ReportingWcf;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports
{
    public class ReportDataManager
    {
        private ReportFilter filter;

        private Dictionary<int, IList<SummaryReportData>> ReportsCountPerTimeSliceDataCache = new Dictionary<int, IList<SummaryReportData>>();
        private object ReportsCountPerTimeSliceDataLock = new object();

        private Dictionary<string, IEnumerable<double>> TableSummaryDataCache = new Dictionary<string, IEnumerable<double>>();
        private object TableSummaryDataLock = new object();

        public ReportDataManager(ReportFilter filter)
        {
            this.filter = filter;
        }

        public Task<IList<SummaryReportData>> GetReportsCountsPerTimeSlice(int timeSliceInMinutes)
        {
            return CheckFilter()
                ? Task.Run(() => (IList<SummaryReportData>) null)
                : Task.Run(() => GetReportsCountsPerTimeSliceFromCache(timeSliceInMinutes));
        }

        public Task<IEnumerable<double>> GetTableSummaryData(string tableName, string rowName, string columnName)
        {
            return CheckFilter()
                ? Task.Run(() => (IEnumerable<double>) null)
                : Task.Run(() => GetTableSummaryDataFromCache(tableName, rowName, columnName));
        }

        private bool CheckFilter()
        {
            return filter == null || string.IsNullOrEmpty(filter.SequenceName);
        }

        private IEnumerable<double> GetTableSummaryDataFromCache(string tableName, string rowName, string columnName)
        {
            lock (TableSummaryDataLock)
            {
                var columnKey = GetTableColumnKey(tableName, rowName, columnName);
                if (!TableSummaryDataCache.ContainsKey(columnKey))
                {
                    TableSummaryDataCache[columnKey] = ReportingClient.GetReportingClient().GetTableSummaryData(filter, tableName, rowName, columnName).Result;
                }
                return TableSummaryDataCache[columnKey];
            }
        }

        private string GetTableColumnKey(string tableName, string rowName, string columnName) => string.Join(".", tableName, rowName, columnName);

        private IList<SummaryReportData> GetReportsCountsPerTimeSliceFromCache(int timeSliceInMinutes)
        {
            lock(ReportsCountPerTimeSliceDataLock)
            {
                if (!ReportsCountPerTimeSliceDataCache.ContainsKey(timeSliceInMinutes))
                {
                    ReportsCountPerTimeSliceDataCache[timeSliceInMinutes] = FillGaps(ReportingClient.GetReportingClient().GetReportsCountsPerTimeSlice(filter, timeSliceInMinutes).Result, timeSliceInMinutes);
                }
                return ReportsCountPerTimeSliceDataCache[timeSliceInMinutes];
            }
        }

        private IList<SummaryReportData> FillGaps(IEnumerable<SummaryReportData> reportData, int timeSliceInMinutes)
        {
            var newCollection = new List<SummaryReportData>();
            TimeSpan timeInterval = new TimeSpan(0, timeSliceInMinutes, 0);
            using (var enumerator = reportData.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    newCollection.Add(enumerator.Current);
                }

                while (enumerator.MoveNext())
                {
                    while ((enumerator.Current.BeginInterval - newCollection.Last().BeginInterval) > timeInterval)
                    {
                        newCollection.Add(new SummaryReportData { BeginInterval = newCollection.Last().BeginInterval + timeInterval });
                    }

                    newCollection.Add(enumerator.Current);
                }
            }

            return newCollection;
        }
    }
}
