using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportViewer;

namespace MTFCore.DbReporting.Configurations
{
    class SummaryReportTextPanelConfiguration : IEntityTypeConfiguration<DbTextPanel>
    {
        public void Configure(EntityTypeBuilder<DbTextPanel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.ToTable("SummaryReportTextPanel");
        }
    }
}