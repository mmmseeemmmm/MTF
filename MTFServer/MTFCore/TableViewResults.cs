using System;
using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFCore
{
    class TableViewResults
    {
        //dutId -> tableId -> list of rows
        private Dictionary<Guid, Dictionary<Guid, List<MTFValidationTableRowResult>>> tableViewResultsInternal = new Dictionary<Guid, Dictionary<Guid, List<MTFValidationTableRowResult>>>();
        private Guid DefaultDUTId = new Guid("FF1E9AE3-4B03-4962-AE77-01A92D0178EB");

        private Dictionary<Guid, List<MTFValidationTableRowResult>> TableViewResultsData(Guid? dutId)
        {
            var dut = dutId ?? DefaultDUTId;
            if (!tableViewResultsInternal.ContainsKey(dut))
            {
                tableViewResultsInternal[dut] = new Dictionary<Guid, List<MTFValidationTableRowResult>>();
            }

            return tableViewResultsInternal[dut];
        }

        public void UpdateTableRows(List<MTFValidationTableRowResult> rows, Guid? dutId)
        {
            if (TableViewResultsData(dutId).ContainsKey(rows[0].TableId))
            {
                //tableViewResults[rows[0].TableId].AddRange(rows);
                var result = TableViewResultsData(dutId)[rows[0].TableId];
                foreach (var rowResult in rows)
                {
                    var r = result.FirstOrDefault(x => x.Row.Id == rowResult.Row.Id);
                    if (r != null)
                    {
                        result[result.IndexOf(r)] = rowResult;
                    }
                    else
                    {
                        result.Add(rowResult);
                    }
                }
            }
            else
            {
                TableViewResultsData(dutId)[rows[0].TableId] = rows;
            }
        }

        public MTFValidationTableRowResult[] GetAllResultsByTables()
        {
            var list = new List<MTFValidationTableRowResult>();
            foreach (var results in tableViewResultsInternal.Values)
            {
                foreach (var l in results.Values)
                {
                    list.AddRange(l);
                }
            }
            return list.ToArray();
        }

        public void Clear(Guid? dutId) => TableViewResultsData(dutId).Clear();

        public void RemoveTable(Guid? dutId, Guid tableId) => TableViewResultsData(dutId).Remove(tableId);
    }
}
