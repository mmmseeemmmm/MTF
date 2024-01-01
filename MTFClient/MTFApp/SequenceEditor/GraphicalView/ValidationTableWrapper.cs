using System;
using System.Collections.Generic;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class ValidationTableWrapper : GraphicalViewTableItemBase
    {
        private MTFValidationTable validationTable;
        private string sequenceName;
        private List<ValidationRowWrapper> rows = new List<ValidationRowWrapper>();
        private bool isCollapsed = true;


        public MTFValidationTable ValidationTable
        {
            get => validationTable;
            set => validationTable = value;
        }

        public string SequenceName
        {
            get => sequenceName;
            set => sequenceName = value;
        }

        public List<ValidationRowWrapper> Rows
        {
            get => rows;
            set => rows = value;
        }

        public override string Alias => ValidationTable?.Name;
        public override Guid TableId => ValidationTable?.Id ?? Guid.Empty;
        public override Guid RowId => Guid.Empty;

        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                isCollapsed = value;
                NotifyPropertyChanged();
            }
        }
    }
}