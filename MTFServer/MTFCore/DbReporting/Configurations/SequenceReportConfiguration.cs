using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFCore.DbReporting.Helpers;

namespace MTFCore.DbReporting.Configurations
{
    class SequenceReportConfiguration : IEntityTypeConfiguration<SequenceReport>
    {
        public void Configure(EntityTypeBuilder<SequenceReport> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasIndex(x => x.SequenceName);
            builder.HasIndex(x => x.SequenceStatus);
            builder.Property(x => x.SequenceName).HasMaxLength(DbConstants.HeaderNameMaxLength).IsRequired();
            builder.Property(x => x.GsRemains).HasMaxLength(DbConstants.HeaderStringMaxLength);
            builder.Property(x => x.CycleName).HasMaxLength(DbConstants.HeaderStringMaxLength);
            builder.ToTable("SequenceReport");
        }
    }
}