using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFCommon;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTableCell : MTFDataTransferObject
    {
        #region ctor

        public MTFValidationTableCell()
            : base()
        {

        }

        public MTFValidationTableCell(MTFValidationTableColumn column)
            : base()
        {
            this.Column = column;
            this.Header = column.Header;
            this.Type = column.Type;
            this.IsSet = column.Type == MTFTableColumnType.GoldSample;
            if (column.Type == MTFTableColumnType.Hidden)
            {
                Value = false;
            }
        }

        public MTFValidationTableCell(SerializationInfo info, StreamingContext context)
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


        public object Value
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

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


        public MTFTableColumnType Type
        {
            get { return GetProperty<MTFTableColumnType>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged("CanRemove");
            }
        }

        public bool IsSet
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool Status
        {
            get
            {
                return Type == MTFTableColumnType.ActualValue || Type == MTFTableColumnType.Identification || Type == MTFTableColumnType.Hidden ||
                       GetProperty<bool>();
            }
            set { SetProperty(value); }
        }

        public bool IsActualParam
        {
            get { return Type == MTFTableColumnType.Value && IsSet; }
        }

        [MTFPersistIdOnly]
        public MTFValidationTableColumn Column
        {
            get { return GetProperty<MTFValidationTableColumn>(); }
            set { SetProperty(value); }
        }

        public bool HasImage
        {
            get { return this.Value is AutomotiveLighting.MTFCommon.MTFImage; }
        }

        private bool isChanged;

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


        #region Public Methods

        public MTFValidationTableStatus Evaluate(object actualValue, Term condition, bool evaluateGoldSample, bool isGoldSample)
        {
            if (condition == null || Column == null)
            {
                Status = true;
                return MTFValidationTableStatus.Ok;
            }
            if (Column.Type == MTFTableColumnType.Identification || Column.Type == MTFTableColumnType.ActualValue || !IsSet)
            {
                Status = true;
                return MTFValidationTableStatus.Ok;
            }
            if (Column.Type == MTFTableColumnType.GoldSample && !evaluateGoldSample)
            {
                Status = true;
                return MTFValidationTableStatus.Ok;
            }

            bool result = false;
            if (condition is GoldSampleTerm)
            {
                ((GoldSampleTerm)condition).ActualValue = GetTermFromValue(actualValue);
                ((GoldSampleTerm)condition).GoldSampleValue = this.Value;
            }
            else if (condition is BinaryLogicalTerm)
            {
                ((BinaryLogicalTerm)condition).Value1 = GetTermFromValue(actualValue);
                ((BinaryLogicalTerm)condition).Value2 = GetTermFromValue(this.Value);
            }
            else if (condition is ListTerm)
            {
                ((ListTerm)condition).Value1 = GetTermFromValue(actualValue);
                ((ListTerm)condition).Value2 = this.Value;

            }
            object eval = condition.Evaluate();
            if (eval != null)
            {
                bool.TryParse(eval.ToString(), out result);
            }
            this.Status = result;
            if (Type == MTFTableColumnType.GoldSample && !result)
            {
                return MTFValidationTableStatus.GSFail;
            }
            return result ? MTFValidationTableStatus.Ok : MTFValidationTableStatus.Nok;
        }

        public string GetValueAsString()
        {
            switch (Value)
            {
                case null:
                    return null;
                case string s:
                    return s;
                case IEnumerable<string> collection:
                    return string.Join("; ", collection);
                default:
                    return Value.ToString();
            }
        }

        #endregion

        #region Private Methods

        private Term GetTermFromValue(object actualValue)
        {
            var term = actualValue as Term;
            return term ?? new ConstantTerm() { Value = actualValue };
        }

        #endregion


    }
}
