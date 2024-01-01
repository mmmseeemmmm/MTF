using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportViewer;

namespace MTFCore.DbReporting.Configurations
{
    class SummaryReportLineChartSeriesConfiguration : IEntityTypeConfiguration<DbLineChartSeries>
    {
        public void Configure(EntityTypeBuilder<DbLineChartSeries> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(255).IsRequired(false);
            builder.Property(x => x.TableName).HasMaxLength(255).IsRequired(false);
            builder.Property(x => x.RowName).HasMaxLength(255).IsRequired(false);
            builder.Property(x => x.ColumnName).HasMaxLength(255).IsRequired(false);
            builder.ToTable("SummaryReportLineChartSeries");
        }
    }
}