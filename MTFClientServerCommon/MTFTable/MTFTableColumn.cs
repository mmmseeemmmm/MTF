using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.MTFTable
{
    [Serializable]
    public class MTFTableColumn : MTFDataTransferObject
    {
        private const double minWidth = 35;
        
        #region ctor

        public MTFTableColumn()
        {
            
        }

        public MTFTableColumn(bool isIdentification, bool canRemove)
        {
            this.IsIdentification = isIdentification;
            this.CanRemove = canRemove;
            this.Width = 120;
        }

        public MTFTableColumn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        #endregion

        #region Properties

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

        public double Width
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public static double MinWidth
        {
            get { return minWidth; }
        }

        public bool CanRemove
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion
    }
}
