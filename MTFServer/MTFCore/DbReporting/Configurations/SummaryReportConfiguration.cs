using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportViewer;

namespace MTFCore.DbReporting.Configurations
{
    class SummaryReportConfiguration: IEntityTypeConfiguration<SummaryReport>
    {
        public void Configure(EntityTypeBuilder<SummaryReport> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Index).IsRequired();
            builder.ToTable("SummaryReportSetting");
        }
    }
}
