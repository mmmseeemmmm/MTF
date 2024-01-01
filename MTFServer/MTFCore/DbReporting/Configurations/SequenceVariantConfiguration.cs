using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFCore.DbReporting.Helpers;

namespace MTFCore.DbReporting.Configurations
{
    class SequenceVariantConfiguration : IEntityTypeConfiguration<ReportSequenceVariant>
    {
        public void Configure(EntityTypeBuilder<ReportSequenceVariant> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.Type);
            builder.Property(x => x.Name).HasMaxLength(DbConstants.HeaderVariantMaxLength).IsRequired();
            builder.Property(x => x.Type).IsRequired();
            builder.ToTable("SequenceVariant");
        }
    }
}