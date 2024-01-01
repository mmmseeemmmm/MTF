using System;
using System.Runtime.Serialization;
using MTFClientServerCommon.MTFTable;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFVariable : MTFDataTransferObject
    {
        public MTFVariable()
            : base()
        {
        }

        public MTFVariable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string TypeName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public object Value
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }

        public bool IsGlobal
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool HasValidationTable => Value is MTFValidationTable.MTFValidationTable;
        
        public bool HasConstantTable => Value is MTFConstantTable;

        
        public bool HasTable => Value is IMTFTable;

        public bool DependsOnDut
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool HasDataTable => Value is IMTFDataTable;
    }
}
