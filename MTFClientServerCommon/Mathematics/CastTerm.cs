using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class CastTerm : UnaryTerm
    {
        private readonly List<Type> types = new List<Type>
                                   {
                                       typeof(bool),
                                       typeof(byte),
                                       typeof(ushort),
                                       typeof(uint),
                                       typeof(ulong),
                                       typeof(sbyte),
                                       typeof(short),
                                       typeof(int),
                                       typeof(long),
                                       typeof(float),
                                       typeof(double),
                                       typeof(decimal),
                                       typeof(char),
                                       typeof(string),
                                   };

        public CastTerm()
            : base()
        {

        }

        public CastTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override object Evaluate()
        {
            if (SelectedType != null && Value!=null)
            {
                var evaluatedValue = Value.Evaluate();

                if (evaluatedValue == null)
                {
                    throw new Exception("It is not possible to cast null value.");
                }

                var stringValue = evaluatedValue.ToString();

                if (SelectedType == typeof(string))
                {
                    return stringValue;
                }
                if (SelectedType == typeof(char))
                {
                    return char.Parse(stringValue);
                }
                if (SelectedType == typeof(bool))
                {
                    return bool.Parse(stringValue);
                }

                string numberString = stringValue.Replace(",", ".");

                if (SelectedType == typeof(double))
                {
                    return double.Parse(numberString, CultureInfo.InvariantCulture);
                }
                if (SelectedType == typeof(float))
                {
                    return float.Parse(numberString, CultureInfo.InvariantCulture);
                }
                if (SelectedType == typeof(decimal))
                {
                    return decimal.Parse(numberString, CultureInfo.InvariantCulture);
                }

                var doubleNumber = double.Parse(numberString, CultureInfo.InvariantCulture);

                if (doubleNumber % 1 != 0)
                {
                    throw new Exception(string.Format("Cannot cast {0} to {1}.", numberString, SelectedType.Name));
                }
                if (SelectedType == typeof(byte))
                {
                    return Convert.ToByte(doubleNumber);
                }
                if (SelectedType == typeof(sbyte))
                {
                    return Convert.ToSByte(doubleNumber);
                }
                if (SelectedType == typeof(ushort))
                {
                    return Convert.ToUInt16(doubleNumber);
                }
                if (SelectedType == typeof(uint))
                {
                    return Convert.ToUInt32(doubleNumber);
                }
                if (SelectedType == typeof(ulong))
                {
                    return Convert.ToUInt64(doubleNumber);
                }
                if (SelectedType == typeof(short))
                {
                    return Convert.ToInt16(doubleNumber);
                }
                if (SelectedType == typeof(int))
                {
                    return Convert.ToInt32(doubleNumber);
                }
                if (SelectedType == typeof(long))
                {
                    return Convert.ToInt64(doubleNumber);
                }
            }

            throw new Exception("Cast term is not valid, please select correct type for casting");
        }



        public override Type ResultType
        {
            get { return SelectedType; }
        }

        public override string Symbol
        {
            get { return "(CAST)"; }
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.Convert; }
        }

        public override string Label
        {
            get { return "Casting"; }
        }

        public IEnumerable<Type> Types
        {
            get { return types; }
        }

        public Type SelectedType
        {
            get { return GetProperty<Type>(); }
            set { SetProperty(value); }
        }
    }
}
