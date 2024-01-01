using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFCore.DbReporting.Helpers;

namespace MTFCore.DbReporting.Configurations
{
    public class ErrorConfiguration : IEntityTypeConfiguration<ReportError>
    {
        public void Configure(EntityTypeBuilder<ReportError> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ActivityName).HasMaxLength(DbConstants.ErrorActivityNameLength);
            builder.ToTable("Error");
        }
    }
}