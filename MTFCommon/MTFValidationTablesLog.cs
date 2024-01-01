using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace MTFCommon
{
    [Serializable]
    [XmlRoot("TableLog")]
    public class MTFValidationTablesLog
    {
        private MTFLogHeader header;

        public MTFLogHeader Header
        {
            get { return header; }
            set { header = value; }
        }

        private List<string> errorImages;

        [XmlArray("ErrorImages")]
        [XmlArrayItem("ErrorImage")]
        public List<string> ErrorImages
        {
            get { return errorImages; }
            set { errorImages = value; }
        }

        private List<string> graphicalViews;

        [XmlArray("GraphicalViews")]
        [XmlArrayItem("GraphicalView")]
        public List<string> GraphicalViews
        {
            get { return graphicalViews; }
            set { graphicalViews = value; }
        }

        private List<MTFValidationTableLog> tables;

        [XmlArray("Tables")]
        [XmlArrayItem("Table")]
        public List<MTFValidationTableLog> Tables
        {
            get { return tables; }
            set { tables = value; }
        }

        private List<MTFErrorLog> errors;

        [XmlArray("Errors")]
        [XmlArrayItem("Error")]
        public List<MTFErrorLog> Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        private List<MTFMessageLog> messages;

        [XmlArray("Messages")]
        [XmlArrayItem("Message")]
        public List<MTFMessageLog> Messages
        {
            get { return messages; }
            set { messages = value; }
        }


        public bool IsEmpty
        {
            get { return (Tables == null || Tables.Count < 1) && (Errors == null || Errors.Count < 1) && (Messages == null || Messages.Count < 1); }
        }

        


        public void AddTableSorted(MTFValidationTableLog tableLog)
        {
            if (Tables == null)
            {
                Tables = new List<MTFValidationTableLog>();
            }
            if (Tables.Count < 1)
            {
                Tables.Add(tableLog);
            }
            else
            {
                var previousTable = Tables.FirstOrDefault(x => x.TimeOfValidation > tableLog.TimeOfValidation);
                if (previousTable != null)
                {
                    var index = Tables.IndexOf(previousTable);
                    Tables.Insert(index, tableLog);
                }
                else
                {
                    Tables.Add(tableLog);
                }
            }
        }
    }

    [Serializable]
    public class MTFValidationTableLog
    {
        private string name;

        [XmlAttribute("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string mode;

        public string ValidationMode
        {
            get { return mode; }
            set { mode = value; }
        }


        private MTFValidationTableStatus status;

        public MTFValidationTableStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        private List<MTFValidationTableRowLog> rows;

        [XmlArray("Rows")]
        [XmlArrayItem("Row")]
        public List<MTFValidationTableRowLog> Rows
        {
            get { return rows; }
            set { rows = value; }
        }

        private DateTime timeOfValidation;

        public DateTime TimeOfValidation
        {
            get { return timeOfValidation; }
            set { timeOfValidation = value; }
        }

    }


    [Serializable]
    public class MTFValidationTableRowLog
    {
        private string name;

        [XmlAttribute("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string actualValue;

        public string ActualValue
        {
            get { return actualValue; }
            set { actualValue = value; }
        }

        private string roundedValue;

        public string RoundedValue
        {
            get { return roundedValue; }
            set { this.roundedValue = value; }
        }


        private MTFValidationTableStatus status;

        public MTFValidationTableStatus Status
        {
            get { return status; }
            set { status = value; }
        }


        private uint numberOfRepetition;

        public uint NumberOfRepetition
        {
            get { return numberOfRepetition; }
            set { numberOfRepetition = value; }
        }

        private bool hasImage;

        public bool HasImage
        {
            get { return hasImage; }
            set { hasImage = value; }
        }



        private List<MTFValidationTableRowCondition> conditions;

        [XmlArray("Conditions")]
        [XmlArrayItem("Condition")]
        public List<MTFValidationTableRowCondition> Conditions
        {
            get { return conditions; }
            set { conditions = value; }
        }
    }

    [Serializable]
    public class MTFValidationTableRowCondition
    {
        private string name;

        [XmlAttribute("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string value;

        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        private string roundedValue;

        public string RoundedValue
        {
            get { return roundedValue; }
            set { this.roundedValue = value; }
        }

        private bool status;

        public bool Status
        {
            get { return status; }
            set { status = value; }
        }

        private bool isSet;

        public bool IsSet
        {
            get { return isSet; }
            set { isSet = value; }
        }

    }

    public class MTFErrorLog
    {
        private string timeStamp;

        public string TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        private ErrorTypes type;

        public ErrorTypes Type
        {
            get { return type; }
            set { type = value; }
        }

        private string activity;

        public string Activity
        {
            get { return activity; }
            set { activity = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        [XmlIgnore]
        public DateTime TimeStampDate
        {
            get { return timeStampDate; }
            set { timeStampDate = value; }
        }

        private DateTime timeStampDate;
    }

    public class MTFMessageLog
    {
        private string timeStamp;

        public string TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }

    public enum ErrorTypes
    {
        CheckOutputValue,
        ComponentError,
        SequenceError,
    }
}
