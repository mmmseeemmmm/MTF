using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MTFClientServerCommon.DbEntities.DbReportViewer;
using MTFClientServerCommon.DbReporting.Helpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFCore.DbReporting.Repositories
{
    class SummaryReportRepository : RepositoryBase
    {
        public List<SummaryReportSettings> GetSummaryReports()
        {
            var summaries = ExecuteOnDbContext(db => db.SummaryReportSetting.ToList());

            var output = EntityMapper.TransformSummaries(summaries);
            output.ForEach(r => r.Panels = new ObservableCollection<PanelBase>(LoadPanels(r.Id)));
            return output;
        }

        public List<SummaryReportSettings> SaveSummarySetting(List<SummaryReportSettings> data)
        {
            ExecuteOnDatabaseContextWithSave(db =>
                                             {
                                                 foreach (var summary in data)
                                                 {
                                                     var dbItem = db.SummaryReportSetting
                                                         .Include(x => x.LineChartPanels).ThenInclude(x => x.Series)
                                                         .Include(x => x.OverviewPanels)
                                                         .Include(x => x.TextPanels)
                                                         .FirstOrDefault(x => x.Id == summary.Id);

                                                     if (dbItem != null)
                                                     {
                                                         EntityMapper.TransformSummary(dbItem, summary);
                                                     }
                                                     else
                                                     {
                                                         var dbSummary = new SummaryReport
                                                                         {
                                                                             LineChartPanels = new List<DbLineChartPanel>(),
                                                                             OverviewPanels = new List<DbReportsOverviewPanel>(),
                                                                             TextPanels = new List<DbTextPanel>(),
                                                                         };
                                                         EntityMapper.TransformSummary(dbSummary, summary);
                                                         db.SummaryReportSetting.Add(dbSummary);
                                                     }
                                                 }
                                             });

            return GetSummaryReports();
        }

        public void RemoveSummarySetting(List<SummaryReportSettings> data)
        {
            ExecuteOnDatabaseContextWithSave(db =>
                                             {
                                                 foreach (var summary in data)
                                                 {
                                                     var dbItem = db.SummaryReportSetting.FirstOrDefault(x => x.Id == summary.Id);

                                                     if (dbItem != null)
                                                     {
                                                         db.SummaryReportSetting.Remove(dbItem);
                                                     }
                                                 }
                                             });
        }

        private List<PanelBase> LoadPanels(int summaryReportId)
        {
            var output = new List<PanelBase>();

            var lineCharts = ExecuteOnDbContext(db => db.SummaryReportLineCharts.Include(x => x.Series)
                                                    .Where(x => x.SummaryReportId == summaryReportId).ToList());
            var overviews = ExecuteOnDbContext(db => db.SummaryReportOverviewPanels.Where(x => x.SummaryReportId == summaryReportId).ToList());
            var textPanels = ExecuteOnDbContext(db => db.SummaryReportTextPanels.Where(x => x.SummaryReportId == summaryReportId).ToList());

            var uiLineCharts = lineCharts.Select(x=>
                                                 {
                                                     var lineChart = EntityMapper.MapEntity<LineChartPanel, DbLineChartPanel>(x);
                                                     lineChart.Series = new ObservableCollection<LineChartSeriesSettings>(lineChart.Series.OrderBy(s => s.Index));
                                                     return lineChart;
                                                 });
            var uiOverviews = overviews.Select(EntityMapper.MapEntity<ReportsOverviewPanel, DbReportsOverviewPanel>);
            var uiTextPanels = textPanels.Select(EntityMapper.MapEntity<TextPanel, DbTextPanel>);

            output.AddRange(uiLineCharts);
            output.AddRange(uiOverviews);
            output.AddRange(uiTextPanels);

            return output.OrderBy(x => x.Index).ToList();
        }
    }
}