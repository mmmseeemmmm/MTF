using MTFClientServerCommon.DbEntities.DbReportEntities;

namespace MTFCore.DbReporting.Repositories
{
    class MessageRepository : RepositoryBase
    {
        public void SaveMessage(ReportMessage message)
        {
            ExecuteOnDatabaseContextWithSave(db => db.Messages.Add(message));
        }
    }
}