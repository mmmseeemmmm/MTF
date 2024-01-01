using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportViewer;

namespace MTFCore.DbReporting.Configurations
{
    class SummaryReportLineChartConfiguration : IEntityTypeConfiguration<DbLineChartPanel>
    {
        public void Configure(EntityTypeBuilder<DbLineChartPanel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.ToTable("SummaryReportLineChartPanel");
        }
    }
}