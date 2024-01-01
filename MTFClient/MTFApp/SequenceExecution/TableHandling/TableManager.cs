using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MTFApp.SequenceExecution.Helpers;
using MTFApp.SequenceExecution.MainViews;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;

namespace MTFApp.SequenceExecution.TableHandling
{
    public class TableManager
    {
        private readonly Dictionary<Guid,ObservableCollection<ExecutionValidTable>> validationTablesInternal = new Dictionary<Guid, ObservableCollection<ExecutionValidTable>>();
        private readonly ObservableCollection<ExecutionValidTable> serviceTables = new ObservableCollection<ExecutionValidTable>();
        private readonly Dictionary<Guid, List<ExecutionValidTable>> existingTablesInternal = new Dictionary<Guid, List<ExecutionValidTable>>();
        private bool allowEditTables;
        private bool isCollapseAllActivated;
        private Guid defaultDutId = DutConstants.DefaultDutId;
        private List<Guid> dutIds;


        private readonly object tableLock = new object();
        private readonly object serviceTableLock = new object();
        private readonly object getServiceTableLock = new object();
        private readonly object getValidationTableLock = new object();

        public ObservableCollection<ExecutionValidTable> ValidationTables(Guid? dutId) => validationTablesInternal[dutId ?? defaultDutId];
        public List<ExecutionValidTable> ExistingTables(Guid? dutId) => existingTablesInternal[dutId ?? defaultDutId];

        public ObservableCollection<ExecutionValidTable> ServiceTables
        {
            get { return serviceTables; }
        }

        public bool IsCollapseAllActivated
        {
            get { return isCollapseAllActivated; }
        }

        public void Init(MTFSequence sequence, bool isCollapseAllActivated)
        {
            if (sequence != null)
            {
                allowEditTables = sequence.ServiceDesignSetting.AllowEditTables;
                if (sequence.DeviceUnderTestInfos != null && sequence.DeviceUnderTestInfos.Count > 0)
                {
                    dutIds = sequence.DeviceUnderTestInfos.Select(d => d.Id).ToList();
                    defaultDutId = sequence.DeviceUnderTestInfos[0].Id;
                    foreach (var dut in sequence.DeviceUnderTestInfos)
                    {
                        validationTablesInternal[dut.Id] = new ObservableCollection<ExecutionValidTable>();
                        existingTablesInternal[dut.Id] = new List<ExecutionValidTable>();
                    }
                }
                else
                {
                    dutIds = new List<Guid>{defaultDutId};
                    validationTablesInternal[defaultDutId] = new ObservableCollection<ExecutionValidTable>();
                    existingTablesInternal[defaultDutId] = new List<ExecutionValidTable>();
                }

                if (existingTablesTmp.Count > 0)
                {
                    foreach (var table in existingTablesTmp)
                    {
                        if (table.DependsOnDut)
                        {
                            foreach (var dutId in existingTablesInternal.Keys)
                            {
                                ExistingTables(dutId).Add(new ExecutionValidTable(table.Name, table.Id, table.IsCollapsed, table.VariableId, table.DependsOnDut));
                            }
                        }
                        else
                        {
                            ExistingTables(defaultDutId).Add(new ExecutionValidTable(table.Name, table.Id, table.IsCollapsed, table.VariableId, table.DependsOnDut));
                        }
                    }
                    existingTablesTmp.Clear();
                }
            }
            this.isCollapseAllActivated = isCollapseAllActivated;
        }


        public ExecutionValidTable GetValidationTable(Guid tableId, Guid? dutId)
        {
            var table = ValidationTables(dutId).FirstOrDefault(x => x.Id == tableId);
            if (table != null)
            {
                return table;
            }
            table = ExistingTables(dutId).FirstOrDefault(x => x.Id == tableId);
            if (table != null)
            {
                Application.Current.Dispatcher.Invoke(() => this.ValidationTables(dutId).Add(table));
                return table;
            }
            return null;
        }


        public void OpenExecutionTable(ExecutionValidTable executionValidTable)
        {
            foreach (var dutId in dutIds)
            {
                foreach (var validTable in ValidationTables(dutId))
                {
                    validTable.IsCollapsed = validTable != executionValidTable;
                }
            }
        }

        public void ClearRowsInValidationTables()
        {
            lock (tableLock)
            {
                foreach (var dutId in validationTablesInternal.Keys)
                {
                    foreach (var t in ValidationTables(dutId))
                    {
                        t.Clear();
                    }

                    ValidationTables(dutId).Clear();
                }
            }
        }


        private ExecutionValidTable GetServiceTable(Guid tableId)
        {
            lock (getServiceTableLock)
            {
                var table = serviceTables.FirstOrDefault(x => x.Id == tableId);
                if (table != null)
                {
                    return table;
                }
                var newTable = ExistingTables(null).FirstOrDefault(x => x.Id == tableId);
                if (newTable != null)
                {
                    table = new ExecutionValidTable(newTable.Name, tableId, isCollapseAllActivated, newTable.VariableId, newTable.DependsOnDut)
                            {
                                IsEditable = allowEditTables
                            };
                    Application.Current.Dispatcher.Invoke(() => serviceTables.Add(table));
                    return table;
                }
                return null;
            }
        }


        public void ClearTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId)
        {
            lock (tableLock)
            {
                if (clearAllTables)
                {
                    App.Current.Dispatcher.Invoke(() =>
                                                  {
                                                      foreach (var t in ValidationTables(dutId))
                                                      {
                                                          t.Clear();
                                                      }
                                                      ValidationTables(dutId).Clear();
                                                      foreach (var table in ServiceTables)
                                                      {
                                                          table.Clear();
                                                      }
                                                      ServiceTables.Clear();
                                                  });
                }
                else
                {
                    tablesForClearing.ForEach(tableGuid =>
                                              {
                                                  App.Current.Dispatcher.Invoke(() =>
                                                                                {
                                                                                    var table =
                                                                                        ValidationTables(dutId).FirstOrDefault(t => t.Id == tableGuid);
                                                                                    if (table != null)
                                                                                    {
                                                                                        table.Clear();
                                                                                        table.Status = MTFValidationTableStatus.NotFilled;
                                                                                    }
                                                                                    ValidationTables(dutId).Remove(table);
                                                                                    table = ServiceTables.FirstOrDefault(t => t.Id == tableGuid);
                                                                                    if (table != null)
                                                                                    {
                                                                                        table.Clear();
                                                                                        table.Status = MTFValidationTableStatus.NotFilled;
                                                                                    }
                                                                                    ServiceTables.Remove(table);
                                                                                });
                                              });
                }
            }
        }

        public void ClearExistingTables()
        {
            foreach (var dutId in existingTablesInternal.Keys)
            {
                existingTablesInternal[dutId].Clear();
            }
            existingTablesInternal.Clear();
        }

        public void NewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, ExecutionViewMode viewMode,
            ExecutionStatus executionStatus, MarkedDebugItems currentMarkedItems, Guid? dutId)
        {
            lock (tableLock)
            {
                if (rows != null && rows.Count > 0)
                {
                    if (executionStatus.IsServiceActivated || executionStatus.IsTeachActivated)
                    {
                        var serviceTable = GetServiceTable(rows.First().TableId);
                        lock (serviceTableLock)
                        {
                            UpdateLocalValidationTable(serviceTable, rows, executionStatus, currentMarkedItems);
                        }
                        return;
                    }
                    var currentTable = GetValidationTable(rows.First().TableId, dutId);

                    if (viewMode == ExecutionViewMode.Table || viewMode == ExecutionViewMode.GraphicalView ||
                        (executionStatus.IsDebugActivated && viewMode == ExecutionViewMode.Tree))
                    {
                        UpdateLocalValidationTable(currentTable, rows, executionStatus, currentMarkedItems); //table view data
                    }
                }
            }
        }

        public ExecutionValidTable GetValidationTableForWatch(List<MTFValidationTableRowResult> rows)
        {
            if (rows != null && rows.Count > 0)
            {
                lock (tableLock)
                {
                    return GetValidationTable(rows.First().TableId, null);
                }
            }
            return null;
        }


        private void UpdateLocalValidationTable(ExecutionValidTable table, IEnumerable<MTFValidationTableRowResult> rows,
            ExecutionStatus executionStatus, MarkedDebugItems currentMarkedItems)
        {
            UpdateLocalValidationTable(table, rows, true, false, executionStatus, currentMarkedItems);
        }

        public void UpdateLocalValidationTable(Guid tableId, IEnumerable<MTFValidationTableRowResult> rows, ExecutionStatus executionStatus,
            MarkedDebugItems currentMarkedItems, Guid? dutId)
        {
            lock (tableLock)
            {
                var table = GetValidationTable(tableId, dutId);
                UpdateLocalValidationTable(table, rows, false, true, executionStatus, currentMarkedItems);
            }
        }

        private void UpdateLocalValidationTable(ExecutionValidTable table, IEnumerable<MTFValidationTableRowResult> rows,
            bool markDebugItems, bool sortRowsByTimestamp, ExecutionStatus executionStatus, MarkedDebugItems currentMarkedItems)
        {
            if (table == null)
            {
                return;
            }
            var markItem = false;
            if (rows != null && rows.Any())
            {
                MTFValidationTableStatus currentStatus = MTFValidationTableStatus.NotFilled;
                if (sortRowsByTimestamp)
                {
                    var timeStamp = DateTime.MinValue;
                    foreach (var row in rows)
                    {
                        if (row.TimeStamp > timeStamp)
                        {
                            timeStamp = row.TimeStamp;
                            currentStatus = row.TableStatus;
                        }
                    }
                }
                else
                {
                    currentStatus = rows.Last().TableStatus;
                }

                Application.Current.Dispatcher.Invoke(() => table.AssignStatus(currentStatus));

                foreach (var row in rows)
                {
                    if (Application.Current != null)
                    {
                        if (executionStatus.IsDebugActivated && markDebugItems)
                        {
                            markItem = true;
                            currentMarkedItems.AddItem(rows.Select(x => x.Row));
                        }
                        if (executionStatus.IsDebugActivated && !markDebugItems)
                        {
                            markItem = currentMarkedItems.ReplaceIfContains(row.Row);
                        }
                        var state = executionStatus.IsDebugActivated || executionStatus.IsTeachActivated || executionStatus.IsServiceActivated;
                        table.AssignRow(row.Row, markItem, state, Application.Current);
                    }
                }
            }
        }

        public void RemoveDynamicTables(Guid variableId)
        {
            lock (getValidationTableLock)
            {
                foreach (var dutId in existingTablesInternal.Keys)
                {
                    var et = ExistingTables(dutId).FirstOrDefault(x => x.VariableId == variableId);
                    if (et != null)
                    {
                        ExistingTables(dutId).Remove(et);
                    }
                }
            }
            lock (tableLock)
            {
                foreach (var dutId in validationTablesInternal.Keys)
                {
                    var vt = ValidationTables(dutId).FirstOrDefault(x => x.VariableId == variableId);
                    if (vt != null)
                    {
                        UIHelper.InvokeOnDispatcher(() => ValidationTables(dutId).Remove(vt));
                    }
                }
            }
            lock (getServiceTableLock)
            {
                var st = serviceTables.FirstOrDefault(x => x.VariableId == variableId);
                if (st != null)
                {
                    UIHelper.InvokeOnDispatcher(() => serviceTables.Remove(st));
                }
            }
        }


        public void CollapseAllTables(bool collapse, ExecutionStatus executionStatus)
        {
            if (executionStatus.IsServiceActivated || executionStatus.IsTeachActivated)
            {
                lock (getServiceTableLock)
                {
                    foreach (var table in serviceTables)
                    {
                        table.IsCollapsed = collapse;
                    }
                }
            }
            else
            {
                lock (getValidationTableLock)
                {
                    foreach (var dutId in existingTablesInternal.Keys)
                    {
                        foreach (var executionValidTable in ExistingTables(dutId))
                        {
                            executionValidTable.IsCollapsed = collapse;
                        }
                    }
                }
            }
            isCollapseAllActivated = !isCollapseAllActivated;
        }

        private List<ExecutionValidTable> existingTablesTmp = new List<ExecutionValidTable>();
        public void AddExistingTable(MTFValidationTable table, Guid variableId, bool dependsOnDut)
        {
            if (table != null)
            {
                existingTablesTmp.Add(new ExecutionValidTable(table.Name, table.Id, isCollapseAllActivated, variableId, dependsOnDut));
            }
        }
    }
}