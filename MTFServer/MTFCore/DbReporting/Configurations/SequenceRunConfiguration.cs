using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFCore.DbReporting.Helpers;

namespace MTFCore.DbReporting.Configurations
{
    class SequenceRunConfiguration : IEntityTypeConfiguration<ReportSequenceRun>
    {
        public void Configure(EntityTypeBuilder<ReportSequenceRun> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SequenceName).HasMaxLength(DbConstants.HeaderNameMaxLength);
            builder.Property(x => x.Machine).HasMaxLength(DbConstants.HeaderNameMaxLength);
            builder.Property(x => x.WinUser).HasMaxLength(DbConstants.HeaderNameMaxLength);
            builder.HasIndex(x => x.SequenceName);
            builder.HasIndex(x => x.Id);
            builder.ToTable("SequenceRun");
        }
    }
}