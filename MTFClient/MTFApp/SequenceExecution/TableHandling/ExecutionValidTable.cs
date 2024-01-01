using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;

namespace MTFApp.SequenceExecution.TableHandling
{
    public class ExecutionValidTable : ExecutionTableBase, ITableStatus
    {
        private ObservableCollection<MTFValidationTableRow> rows;
        private MTFValidationTableStatus status;
        private object rowsLock = new object();
        private readonly Guid variableId;

        public ExecutionValidTable(string name, Guid id, bool isCollapsed, Guid variableId, bool dependsOnDut)
        {
            this.variableId = variableId;
            this.Id = id;
            this.Name = name;
            this.rows = new ObservableCollection<MTFValidationTableRow>();
            this.IsCollapsed = isCollapsed;
            this.DependsOnDut = dependsOnDut;
        }

        public MTFValidationTableStatus Status
        {
            get => status;
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<MTFValidationTableRow> Rows => rows;

        public Guid VariableId => variableId;

        public bool DependsOnDut { get; set; }

        public void AddRow(MTFValidationTableRow row)
        {
            lock (rowsLock)
            {
                this.rows.Add(row);
            }
        }

        public void InsertRow(int index, MTFValidationTableRow row)
        {
            lock (rowsLock)
            {
                if (this.rows.Count > index && index >= 0)
                {
                    this.rows.Insert(index, row);
                }
                else
                {
                    AddRow(row);
                }
            }
        }



        public void AssignRow(MTFValidationTableRow row, bool markChangedValue, bool debugMode, Application application)
        {
            bool addRow = false;
            if (row == null)
            {
                return;
            }
            if (debugMode)
            {
                row = row.PrepareForDebug();
            }

            lock(rowsLock)
            { 
                if (this.rows.Count > 0)
                {
                    var originalRow = this.rows.FirstOrDefault(x => x.Id == row.Id);   
                    if (originalRow != null)
                    {
                        int index = this.rows.IndexOf(originalRow);
                        if (index != -1 && index < this.rows.Count)
                        {
                            if (application != null)
                            {
                                application.Dispatcher.Invoke(() => this.rows[index] = row);
                            }
                            else
                            {
                                this.rows[index] = row;
                            }
                        }
                    }
                    else
                    {
                        addRow = true;
                    }
                }
                else
                {
                    addRow = true;
                }
                if (addRow)
                {
                    if (application != null)
                    {
                        application.Dispatcher.Invoke(() => this.rows.Add(row));
                    }
                    else
                    {
                        this.rows.Add(row);
                    }
                }
                if (markChangedValue)
                {
                    row.MarkChangedValue();
                }
            }
        }

        public void AssignStatus(MTFValidationTableStatus status)
        {
            this.Status = status;
        }

        public override void Clear()
        {
            if (Rows != null && Rows.Count > 0)
            {
                lock (rowsLock)
                {
                    Rows.Clear();
                }
            }
            this.Status = MTFValidationTableStatus.NotFilled;
        }


        public MTFImage GetFirstNokImage()
        {
            if (Rows!=null)
            {
                var nokRow = Rows.FirstOrDefault(x => x.IsActualValueImage && x.Status == MTFValidationTableStatus.Nok);
                if (nokRow!=null)
                {
                    return nokRow.GetActualValue() as MTFImage;
                }
            }
            return null;
        }

        public List<MTFImage> GetAllNokImages()
        {
            return Rows != null
                ? Rows.Where(x => x.IsActualValueImage && x.Status == MTFValidationTableStatus.Nok)
                    .Select(x => x.GetActualValue() as MTFImage)
                    .ToList()
                : null;
        }
    }
}
