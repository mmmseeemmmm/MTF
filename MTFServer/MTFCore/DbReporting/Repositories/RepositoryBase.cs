using System;
using MTFClientServerCommon;

namespace MTFCore.DbReporting.Repositories
{
    internal abstract class RepositoryBase
    {
        protected void ExecuteOnDatabaseContext(Action<DbReportingContext> action)
        {
            ExecuteOnDatabaseContext(action, false);
        }

        protected void ExecuteOnDatabaseContextWithSave(Action<DbReportingContext> action)
        {
            ExecuteOnDatabaseContext(action, true);
        }

        private void ExecuteOnDatabaseContext(Action<DbReportingContext> action, bool save)
        {
            using (var db = new DbReportingContext())
            {
                try
                {
                    action(db);

                    if (save)
                    {
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    SystemLog.LogException(e);
                    throw;
                }
            }
        }

        protected T ExecuteOnDbContext<T>(Func<DbReportingContext, T> action)
        {
            using (var db = new DbReportingContext())
            {
                try
                {
                    return action(db);
                }
                catch (Exception e)
                {
                    SystemLog.LogException(e);
                    throw;
                }
            }
        }
    }
}