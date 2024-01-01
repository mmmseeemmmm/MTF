using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MTFClientServerCommon.DbEntities.DbReportEntities;

namespace MTFCore.DbReporting.Configurations
{
    class RoundingRulesConfiguration : IEntityTypeConfiguration<ReportRoundingRules>
    {
        public void Configure(EntityTypeBuilder<ReportRoundingRules> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("RoundingRule");
        }
    }
}