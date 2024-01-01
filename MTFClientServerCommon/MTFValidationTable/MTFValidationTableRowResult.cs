using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTableRowResult : IIdentifier, ICloneable
    {
        private MTFCommon.MTFValidationTableStatus tableStatus;

        public MTFCommon.MTFValidationTableStatus TableStatus
        {
            get { return tableStatus; }
            set { tableStatus = value; }
        }

        private DateTime timeStamp;

        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        private bool hasTimeStamp;

        public bool HasTimeStamp
        {
            get { return hasTimeStamp; }
            set { hasTimeStamp = value; }
        }

        private string tableName;

        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }


        private Guid tableId;

        public Guid TableId
        {
            get { return tableId; }
            set { tableId = value; }
        }

        private Guid? dutId;

        public Guid? DutId
        {
            get => dutId;
            set => dutId = value;
        }

        private bool isValidated;

        public bool IsValidated
        {
            get { return isValidated; }
            set { isValidated = value; }
        }
        



        private MTFValidationTableRow row;

        public MTFValidationTableRow Row
        {
            get { return row; }
            set { row = value; }
        }

        private Guid id = Guid.NewGuid();
        public Guid Id
        {
            get { return id; }
        }

        public object Clone()
        {
            return new MTFValidationTableRowResult
            {
                id = this.id,
                hasTimeStamp = this.hasTimeStamp,
                isValidated = this.isValidated,
                row = this.row,
                tableId = this.tableId,
                tableName = this.tableName,
                tableStatus = this.tableStatus,
                timeStamp = this.timeStamp,
                dutId = this.dutId,
            };
        }
    }
}
