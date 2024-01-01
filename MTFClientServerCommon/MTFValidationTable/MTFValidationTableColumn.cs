using MTFClientServerCommon.Mathematics;
using System;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTableColumn : MTFDataTransferObject
    {
        private ValidationTableCondition selectedCondition;
        private const double minWidth = 35;

        #region ctor
        public MTFValidationTableColumn()
            : base()
        {

        }

        public MTFValidationTableColumn(MTFTableColumnType type)
            : base()
        {
            this.Type = type;
            this.Width = 150;
            if (type == MTFTableColumnType.GoldSample)
            {
                ValidationTerm = new GoldSampleTerm();
            }
        }

        public MTFValidationTableColumn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion
        {
            get { return "1.0.2"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                if (properties != null)
                {
                    bool value;
                    if (properties.ContainsKey("IsIdentification"))
                    {
                        if (bool.TryParse(properties["IsIdentification"].ToString(), out value) && value)
                        {
                            Type = MTFTableColumnType.Identification;
                        }
                    }
                    if (properties.ContainsKey("IsActualValue"))
                    {
                        if (bool.TryParse(properties["IsActualValue"].ToString(), out value) && value)
                        {
                            Type = MTFTableColumnType.ActualValue;
                        }
                    }
                }
                fromVersion = "1.0.1";
            }
            if (fromVersion == "1.0.1")
            {
                DisplayKey = ValidationTableHelper.SetDisplayKey(Header);
                fromVersion = "1.0.2";
            }
        }

        #endregion

        #region Properties

        public string Header
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                DisplayKey = ValidationTableHelper.SetDisplayKey(value);
            }
        }

        public string DisplayKey
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool CanRemove
        {
            get { return Type == MTFTableColumnType.Value; }
        }

        public Term ValidationTerm
        {
            get { return GetProperty<Term>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("HasListTerm");
            }
        }

        public MTFTableColumnType Type
        {
            get { return GetProperty<MTFTableColumnType>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("CanRemove");
            }
        }

        public bool IsActualValue
        {
            get { return Type == MTFTableColumnType.ActualValue; }
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

        public bool IsControlElement
        {
            get
            {
                return Type == MTFTableColumnType.Identification || Type == MTFTableColumnType.ActualValue ||
                    Type == MTFTableColumnType.Hidden;
            }
        }

        public bool HasListTerm
        {
            get { return ValidationTerm is ListTerm; }
        }

        public ValidationTableCondition SelectedCondition
        {
            get { return selectedCondition; }
            set
            {
                selectedCondition = value;
                if (value != null)
                {
                    ValidationTerm = value.Term;
                }
            }
        }


        #endregion

        public void SetPercentage(double percentage)
        {
            if (Type == MTFTableColumnType.GoldSample && ValidationTerm is GoldSampleTerm)
            {
                ((GoldSampleTerm)ValidationTerm).Percentage = percentage;
            }
        }
    }
}
