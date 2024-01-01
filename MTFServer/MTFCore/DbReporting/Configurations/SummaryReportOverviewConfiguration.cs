using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportViewer;

namespace MTFCore.DbReporting.Configurations
{
    class SummaryReportOverviewConfiguration : IEntityTypeConfiguration<DbReportsOverviewPanel>
    {
        public void Configure(EntityTypeBuilder<DbReportsOverviewPanel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.ToTable("SummaryReportOverviewPanel");
        }
    }
}