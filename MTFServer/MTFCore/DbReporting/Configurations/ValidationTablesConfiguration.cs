using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFCore.DbReporting.Helpers;

namespace MTFCore.DbReporting.Configurations
{
    public class ValidationTablesConfiguration : IEntityTypeConfiguration<ReportValidationTable>
    {
        public void Configure(EntityTypeBuilder<ReportValidationTable> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name)
                .HasMaxLength(DbConstants.TableNameLength)
                .IsRequired();
            builder.HasIndex(x => x.Name);
            builder.ToTable("ValidationTable");
        }
    }
}