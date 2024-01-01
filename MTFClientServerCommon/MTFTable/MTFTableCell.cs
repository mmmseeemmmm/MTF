using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.MTFTable
{
    [Serializable]
    public class MTFTableCell : MTFDataTransferObject
    {
        private bool isChanged;
        
        #region ctor

        public MTFTableCell()
        {

        }

        public MTFTableCell(MTFTableColumn column)
        {
            this.Column = column;
            this.Header = column.Header;
            this.IsIdentification = column.IsIdentification;
        }

        public MTFTableCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion


        #region Properties

        public object Value
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public string Header
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsIdentification
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }


        [MTFPersistIdOnly]
        public MTFTableColumn Column
        {
            get { return GetProperty<MTFTableColumn>(); }
            set { SetProperty(value); }
        }

        public bool HasImage
        {
            get { return this.Value is AutomotiveLighting.MTFCommon.MTFImage; }
        }

        public bool IsChanged
        {
            get { return isChanged; }
            set
            {
                isChanged = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

    }
}
