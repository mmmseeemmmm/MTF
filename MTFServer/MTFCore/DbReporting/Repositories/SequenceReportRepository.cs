using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.Helpers;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFCore.DbReporting.Repositories
{
    class SequenceReportRepository : RepositoryBase
    {
        public void CreateSequenceRun(ReportSequenceRun sequenceRun)
        {
            ExecuteOnDatabaseContextWithSave(db => db.SequenceRuns.Add(sequenceRun));
        }

        public void SaveSequenceRun(int sequenceRunId, DateTime stopTime)
        {
            ExecuteOnDatabaseContextWithSave(db =>
                                             {
                                                 var seqRun = db.SequenceRuns.FirstOrDefault(x => x.Id == sequenceRunId);
                                                 if (seqRun != null)
                                                 {
                                                     seqRun.StopTime = stopTime;
                                                 }
                                             });
        }

        public void CreateReport(SequenceReport sequenceReport)
        {
            ExecuteOnDatabaseContextWithSave(db => db.Reports.Add(sequenceReport));
        }

        public void SaveReport(SequenceReport reportToSave, string cycleName, bool logHiddenRows)
        {
            ExecuteOnDatabaseContextWithSave(db =>
                                             {
                                                 var dbReport = db.Reports.FirstOrDefault(x => x.Id == reportToSave.Id);
                                                 if (dbReport != null)
                                                 {
                                                     dbReport.StartTime = reportToSave.StartTime;
                                                     dbReport.StopTime = reportToSave.StopTime;
                                                     dbReport.GsRemains = reportToSave.GsRemains;
                                                     dbReport.GsWarning = reportToSave.GsWarning;
                                                     dbReport.SequenceStatus = reportToSave.SequenceStatus;
                                                     dbReport.VariantVersion = AssignSequenceVariant(reportToSave.VariantVersion, db);
                                                     dbReport.VariantLightDistribution = AssignSequenceVariant(reportToSave.VariantLightDistribution, db);
                                                     dbReport.VariantMountingSide = AssignSequenceVariant(reportToSave.VariantMountingSide, db);
                                                     dbReport.VariantGsDut = AssignSequenceVariant(reportToSave.VariantGsDut, db);
                                                     dbReport.CycleName = cycleName;
                                                     dbReport.ShowHiddenRows = logHiddenRows;
                                                     dbReport.GraphicalViews = reportToSave.GraphicalViews;
                                                     dbReport.Errors = reportToSave.Errors;
                                                 }
                                             });
        }

        private ReportSequenceVariant AssignSequenceVariant(ReportSequenceVariant sequenceVariant, DbReportingContext db)
        {
            if (sequenceVariant == null)
            {
                return null;
            }

            var dbVariant = db.SequenceVariants.FirstOrDefault(x => x.Type == sequenceVariant.Type && x.Name == sequenceVariant.Name);

            return dbVariant ?? sequenceVariant;
        }


        public void RemoveReport(SequenceReport sequenceReport)
        {
            ExecuteOnDatabaseContextWithSave(db => db.Reports.Remove(sequenceReport));
        }

        public List<string> GetSequenceNames()
        {
            return ExecuteOnDbContext(db => db.Reports.Select(x => x.SequenceName).Distinct().ToList());
        }

        public int GetReportsCount(ReportFilter filter)
        {
            return ExecuteOnDbContext(db => BuildSequenceReportQuery(db.Reports.AsQueryable(), filter).Count());
        }

        public IEnumerable<SequenceReportPreview> GetReports(ReportFilter filter)
        {
            return ExecuteOnDbContext(db =>
                {
                    var reports = BuildSequenceReportQuery(db.Reports.AsQueryable(), filter)
                        .OrderByDescending(r => r.StartTime)
                        .Skip(filter.PageSize * filter.PageNumber)
                        .Take(filter.PageSize);

                    return EntityMapper.GetReportPreviews(reports).ToList();
                }
            );
        }

        public IEnumerable<SummaryReportData> GetReportsCountsPerTimeSlice(ReportFilter filter, int timeSliceInMinutes)
        {
            TimeSpan interval = new TimeSpan(0, timeSliceInMinutes, 0);
            return ExecuteOnDbContext(db =>BuildSequenceReportQuery(db.Reports.AsQueryable(), filter)
                    .OrderByDescending(r => r.StartTime)
                    .GroupBy(r => r.StartTime.Ticks / interval.Ticks)
                    .Select(s => new SummaryReportData { Count = s.Count(), Ok = s.Count(r => r.SequenceStatus == true), Nok = s.Count(r => r.SequenceStatus == false), BeginInterval = RoundDown(s.First().StartTime, interval)})
                    .ToList());
        }

        public IEnumerable<double> GetTableSummaryData(ReportFilter filter, string tableName, string rowName, string columnName)
        { 
            return ExecuteOnDbContext(db =>
                {
                    var reports = BuildSequenceReportQuery(db.Reports.AsQueryable(), filter)
                        .SelectMany(r => r.ValidationTables).Where(v => v.Name == tableName).SelectMany(r => r.Rows).Where(r => r.Name == rowName);

                    if (columnName == "ActualValue")
                        return reports.Select(c => StringToDouble(c.ActualValue)).ToList();
                    else if (columnName == "MaxValue")
                        return reports.Select(c => StringToDouble(c.MaxValue)).ToList();
                    else if (columnName == "MinValue")
                        return reports.Select(c => StringToDouble(c.MinValue)).ToList();
                    else if (columnName == "GsValue")
                        return reports.Select(c => StringToDouble(c.GsValue)).ToList();

                    return (IEnumerable<double>) null;
                }
            );
        }

        private double StringToDouble(string value)
        {
            double.TryParse(value, out var d);
            return d;
        }

        private DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }

        private DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks / d.Ticks) * d.Ticks, dt.Kind);
        }

        private IQueryable<SequenceReport> BuildSequenceReportQuery(IQueryable<SequenceReport> source, ReportFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.SequenceName))
            {
                source = FilterByName(source, filter.SequenceName);
            }

            if (!string.IsNullOrEmpty(filter.CycleName))
            {
                source = FilterByCycleName(source, filter.CycleName);
            }

            if (filter.StartTimeFrom.HasValue)
            {
                source = FilterByStartTimeFrom(source, filter.StartTimeFrom.Value);
            }

            if (filter.StartTimeTo.HasValue)
            {
                source = FilterByStartTimeTo(source, filter.StartTimeTo.Value);
            }

            if (filter.Last24Hours)
            {
                source = FilterByStartTimeLastDays(source, 1);
            }

            if (filter.LastWeek)
            {
                source = FilterByStartTimeLastDays(source, 7);
            }

            if (filter.ReportStatus.HasValue)
            {
                source = FilterByStatus(source, filter.ReportStatus.Value);
            }

            return source;
        }


        private IQueryable<SequenceReport> FilterByName(IQueryable<SequenceReport> source, string sequenceName) => source.Where(x => x.SequenceName == sequenceName);
        private IQueryable<SequenceReport> FilterByCycleName(IQueryable<SequenceReport> source, string cycleName) => source.Where(x => x.CycleName.Contains(cycleName));
        private IQueryable<SequenceReport> FilterByStartTimeFrom(IQueryable<SequenceReport> source, DateTime startTimeFrom) => source.Where(x => x.StartTime > startTimeFrom);
        private IQueryable<SequenceReport> FilterByStartTimeTo(IQueryable<SequenceReport> source, DateTime startTimeTo) => source.Where(x => x.StartTime < startTimeTo);
        private IQueryable<SequenceReport> FilterByStartTimeLastDays(IQueryable<SequenceReport> source, int days) => source.Where(x => x.StartTime > DateTime.Now.AddDays(-days));
        private IQueryable<SequenceReport> FilterByStatus(IQueryable<SequenceReport> source, bool status) => source.Where(x => x.SequenceStatus == status);

        public SequenceReportDetail GetDetail(int reportId)
        {
            var output = new SequenceReportDetail();
            var report = ExecuteOnDbContext(db => db.Reports
                                                .Include(x => x.Messages)
                                                .Include(x => x.Errors)
                                                .Include(x=>x.VariantVersion)
                                                .Include(x=>x.VariantMountingSide)
                                                .Include(x=>x.VariantLightDistribution)
                                                .Include(x=>x.VariantGsDut)
                                                .Include(x => x.ValidationTables)
                                                .ThenInclude(x => x.Rows)
                                                .FirstOrDefault(x => x.Id == reportId));

            if (report != null)
            {
                EntityMapper.FillSequenceReportDetail(output, report);

                var run = ExecuteOnDbContext(db => db.SequenceRuns.Include(x=>x.RoundingRules).FirstOrDefault(x => x.Id == report.SequenceRunId));

                if (run != null)
                {
                    output.WinUser = run.WinUser;
                    output.Machine = run.Machine;
                    output.RoundingRules = run.RoundingRules?.Select(x => new RoundingRuleUi
                                                                          {
                                                                              Min = x.Min,
                                                                              Max = x.Max,
                                                                              Digits = x.Digits,
                                                                          }).ToList();
                }
            }

            return output;
        }
    }
}