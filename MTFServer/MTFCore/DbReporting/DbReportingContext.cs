using System.IO;
using Microsoft.EntityFrameworkCore;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFClientServerCommon.DbEntities.DbReportViewer;
using MTFClientServerCommon.Helpers;
using MTFCore.DbReporting.Configurations;

namespace MTFCore.DbReporting
{
    public class DbReportingContext : DbContext
    {
        public DbSet<SequenceReport> Reports { get; set; }
        public DbSet<ReportMessage> Messages { get; set; }
        public DbSet<ReportValidationTable> ValidationTables { get; set; }
        public DbSet<ReportValidationTableRow> Rows { get; set; }
        public DbSet<ReportSequenceRun> SequenceRuns { get; set; }
        public DbSet<ReportSequenceVariant> SequenceVariants { get; set; }


        public DbSet<SummaryReport> SummaryReportSetting { get; set; }
        public DbSet<DbLineChartPanel> SummaryReportLineCharts { get; set; }
        public DbSet<DbReportsOverviewPanel> SummaryReportOverviewPanels { get; set; }
        public DbSet<DbTextPanel> SummaryReportTextPanels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dataFileName = Path.Combine(BaseConstants.DataPath, BaseConstants.LogsBasePath, BaseConstants.LogsDbFileName);

            FileHelper.CreateEmptyFile(dataFileName);
            optionsBuilder.UseSqlite($"Data Source={dataFileName}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ErrorConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new RoundingRulesConfiguration());
            modelBuilder.ApplyConfiguration(new SequenceVariantConfiguration());
            modelBuilder.ApplyConfiguration(new ValidationTablesConfiguration());
            modelBuilder.ApplyConfiguration(new ValidationTableRowConfiguration());
            modelBuilder.ApplyConfiguration(new SequenceReportConfiguration());
            modelBuilder.ApplyConfiguration(new SequenceRunConfiguration());

            modelBuilder.ApplyConfiguration(new SummaryReportConfiguration());
            modelBuilder.ApplyConfiguration(new SummaryReportLineChartConfiguration());
            modelBuilder.ApplyConfiguration(new SummaryReportOverviewConfiguration());
            modelBuilder.ApplyConfiguration(new SummaryReportTextPanelConfiguration());
            modelBuilder.ApplyConfiguration(new SummaryReportLineChartSeriesConfiguration());
        }
    }
}