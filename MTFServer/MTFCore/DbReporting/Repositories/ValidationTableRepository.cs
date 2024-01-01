using System.Collections.Generic;
using MTFClientServerCommon.DbEntities.DbReportEntities;

namespace MTFCore.DbReporting.Repositories
{
    class ValidationTableRepository : RepositoryBase
    {
        public void SaveTable(ReportValidationTable validationTable)
        {
            ExecuteOnDatabaseContextWithSave(db => db.ValidationTables.Add(validationTable));
        }

        public void SaveRows(List<ReportValidationTableRow> rows)
        {
            ExecuteOnDatabaseContextWithSave(db => db.Rows.AddRange(rows));
        }
    }
}