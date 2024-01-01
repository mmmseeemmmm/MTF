using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportSequenceVariant : DbEntity
    {
        public string Name { get; set; }
        public DbSequenceVariantType Type { get; set; }
    }
}