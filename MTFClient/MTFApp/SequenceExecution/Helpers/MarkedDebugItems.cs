using System.Collections.Generic;
using System.Linq;
using MTFApp.SequenceExecution.SharedControls;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceExecution.Helpers
{
    public class MarkedDebugItems
    {
        private object variablesLock = new object();
        private object rowsLock = new object();
        private List<VariablesWatch> variables;
        private List<MTFValidationTableRow> rows;

        public MarkedDebugItems()
        {
            variables = new List<VariablesWatch>();
            rows = new List<MTFValidationTableRow>();
        }

        public void MarkItems()
        {
            MarkItems(true);
        }

        public void UnmarkItems()
        {
            MarkItems(false);
        }

        public void ClearAndUnmark()
        {
            UnmarkItems();
            lock (variablesLock)
            {
                variables.Clear();
            }
            lock (rowsLock)
            {
                rows.Clear();
            }
        }

        public void AddItem(VariablesWatch item)
        {
            lock (variablesLock)
            {
                if (variables.All(x => x.Variable.Id != item.Variable.Id))
                {
                    variables.Add(item); 
                }
            }
        }

        public void AddItem(IEnumerable<MTFValidationTableRow> items)
        {
            lock (rowsLock)
            {
                foreach (var row in items)
                {
                    if (rows.All(x => x.Id != row.Id))
                    {
                        rows.Add(row);
                    }
                }
            }
        }


        private void MarkItems(bool value)
        {
            lock (variablesLock)
            {
                foreach (var variable in variables)
                {
                    variable.IsChanged = value;
                }
            }

            lock (rowsLock)
            {
                foreach (var row in rows)
                {
                    if (value)
                    {
                        row.MarkChangedValue();
                    }
                    else
                    {
                        row.UnmarkChangedValue();
                    }
                }
            }
        }

        public bool ReplaceIfContains(MTFValidationTableRow row)
        {
            lock (rowsLock)
            {
                if (rows!=null && row!=null)
                {
                    var r = rows.FirstOrDefault(x => x.Id == row.Id);
                    if (r!=null)
                    {
                        rows[rows.IndexOf(r)] = row;
                        return true;
                    }
                }
                return false; 
            }
        }
    }
}
