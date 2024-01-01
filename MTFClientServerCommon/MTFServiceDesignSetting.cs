using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFServiceDesignSetting : MTFDataTransferObject
    {
        public MTFServiceDesignSetting()
        {
            TeachSetting = new MTFServiceDesignValues();
            ServiceSetting = new MTFServiceDesignValues();
            ShowLabels = true;
            AllowEditTables = true;
            AllowGSPanel = true;
            HideHeaderInService = false;
            HideHeaderInTeach = false;
        }

        public MTFServiceDesignSetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public MTFServiceDesignValues TeachSetting
        {
            get { return GetProperty<MTFServiceDesignValues>(); }
            set { SetProperty(value); }
        }

        public MTFServiceDesignValues ServiceSetting
        {
            get { return GetProperty<MTFServiceDesignValues>(); }
            set { SetProperty(value); }
        }

        public bool ShowLabels
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowEditTables
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowGSPanel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool HideHeaderInService
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool HideHeaderInTeach
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }

    [Serializable]
    public class MTFServiceDesignValues : MTFDataTransferObject
    {
        public MTFServiceDesignValues()
        {
            
        }

        public MTFServiceDesignValues(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public bool ManualPlacement
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int ColumnsCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int RowsCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

    }

}
