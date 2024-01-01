using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;

namespace MTFCore.DbReporting.Configurations
{
    class MessageConfiguration : IEntityTypeConfiguration<ReportMessage>
    {
        public void Configure(EntityTypeBuilder<ReportMessage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Message");
        }
    }
}