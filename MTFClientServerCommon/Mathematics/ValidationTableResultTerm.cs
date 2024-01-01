using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MTFClientServerCommon.MTFTable;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ValidationTableResultTerm : Term
    {
        public ValidationTableResultTerm()
            : base()
        {
        }

        public ValidationTableResultTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [MTFPersistIdOnly]
        public IMTFTable ValidationTable
        {
            get { return GetProperty<IMTFTable>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("IsTableSelected");
            }
        }

        public Guid SelectedRowId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        public string SelectedColumn
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsTableSelected
        {
            get { return ValidationTable != null; }
        }

        public SequenceVariant SequenceVariant { get; set; }

        public override object Evaluate()
        {
            if (InjectedValue == null)
            {
                throw new Exception("Table is not selected.");
            }
            var rowId = SelectedRowId;
            if (SelectedResultType != ValidationTableResultType.TableResult || InjectedValue is IMTFDataTable)
            {
                if (!InjectedValue.ExistRow(rowId))
                {
                    var rowName = ValidationTable.GetRowNameById(rowId);
                    rowId = InjectedValue.GetRowIdByName(rowName);
                }
            }

            return InjectedValue.GetResult(SelectedResultType, rowId, SelectedColumn, SequenceVariant);
        }

        public override Type ResultType
        {
            get
            {
                if (SelectedResultType == ValidationTableResultType.CellResult || ValidationTable is IMTFDataTable)
                {
                    return typeof(string);
                }
                return typeof(MTFCommon.MTFValidationTableStatus);
            }
        }

        public override string Symbol
        {
            get { return "Table Result"; }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                       | TermGroups.LogicalTerm
                       | TermGroups.NumberTerm
                       | TermGroups.ObjectTerm
                       | TermGroups.StringTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.ValidationTable; }
        }

        public override string Label
        {
            get { return "Table Result"; }
        }

        public override string ToString()
        {
            StringBuilder output;
            if (ValidationTable == null)
            {
                return "Table: Null";
            }
            output = new StringBuilder();
            output.Append("Table: ").Append(ValidationTable.Name);
            if (SelectedRowId != Guid.Empty)
            {
                var validTable = ValidationTable as MTFValidationTable.MTFValidationTable;
                if (validTable != null)
                {
                    if (validTable.Rows != null)
                    {
                        var row = validTable.Rows.FirstOrDefault(x => x.Id == SelectedRowId);
                        if (row != null)
                        {
                            output.Append('.').Append(row.Items.First().Value);
                        }
                    }
                }
                else
                {
                    var dataTable = ValidationTable as IMTFDataTable;
                    if (dataTable != null)
                    {
                        if (dataTable.Rows != null)
                        {
                            var row = dataTable.Rows.FirstOrDefault(x => x.Id == SelectedRowId);
                            if (row != null)
                            {
                                output.Append('.').Append(row.Items.First().Value);
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(SelectedColumn))
            {
                output.Append('.').Append(SelectedColumn);
            }
            return output.ToString();
        }

        public ValidationTableResultType SelectedResultType
        {
            get { return GetProperty<ValidationTableResultType>(); }
            set
            {
                SetProperty(value);
                if (value == ValidationTableResultType.TableResult)
                {
                    SelectedRowId = Guid.Empty;
                    SelectedColumn = null;
                }
            }
        }

        public IEnumerable<EnumValueDescription> ValidationTableResultTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<ValidationTableResultType>(); }
        }

        public IMTFTable InjectedValue { get; set; }
    }
}