using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;

namespace MTFCore.DbReporting.Configurations
{
    class ValidationTableRowConfiguration : IEntityTypeConfiguration<ReportValidationTableRow>
    {
        public void Configure(EntityTypeBuilder<ReportValidationTableRow> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasIndex(x => x.Name);
            builder.ToTable("ValidationTableRow");
        }
    }
}