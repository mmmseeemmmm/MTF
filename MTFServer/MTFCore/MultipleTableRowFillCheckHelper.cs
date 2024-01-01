using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MTFCore
{
    class MultipleTableRowFillCheckHelper
    {
        private ConcurrentDictionary<Guid?, Dictionary<Guid,Guid>> usedValidationTableRowsInternal = new ConcurrentDictionary<Guid?, Dictionary<Guid, Guid>>();
        private Guid DefaultDUTId = new Guid("FF1E9AE3-4B03-4962-AE77-01A92D0178EB");

        private Dictionary<Guid, Guid> UsedValidationTableRows(Guid? dutId)
        {
            var dut = dutId ?? DefaultDUTId;
            if (!usedValidationTableRowsInternal.ContainsKey(dut))
            {
                usedValidationTableRowsInternal[dut] = new Dictionary<Guid, Guid>();
            }

            return usedValidationTableRowsInternal[dut];
        }
        public void Add(Guid? dutId, Guid rowId, Guid activityId)
        {
            UsedValidationTableRows(dutId)[rowId] = activityId;
        }

        public bool IsRowUsed(Guid? dutId, Guid rowId) => UsedValidationTableRows(dutId).ContainsKey(rowId);

        public Guid GetActivityOfUsage(Guid? dutId, Guid rowId) => UsedValidationTableRows(dutId)[rowId];

        public void RemoveValidatedRowsId(Guid? dutId, IEnumerable<Guid> rowIds)
        {
            foreach (var rowId in rowIds)
            {
                UsedValidationTableRows(dutId).Remove(rowId);
            }
        }
    }
}
