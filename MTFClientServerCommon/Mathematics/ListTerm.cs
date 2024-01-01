using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public abstract class ListTerm : Term
    {
        public ListTerm()
            : base()
        {
        }

        public ListTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public virtual Term Value1
        {
            get { return GetProperty<Term>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                {
                    ValidateList();
                }
            }
        }


        public virtual object Value2
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public override object Evaluate()
        {
            var comparable1 = Value1.Evaluate() as IComparable;
            ICollection collectionToCompare = null;
            if (Value2 is ActivityResultTerm && (Value2 as ActivityResultTerm).ActivityResult is ICollection)
            {
                collectionToCompare = (Value2 as ActivityResultTerm).ActivityResult as ICollection;
            }
            else if (Value2 is ICollection)
            {
                collectionToCompare = Value2 as ICollection;
            }
            if (collectionToCompare != null)
            {
                foreach (object o in collectionToCompare)
                {
                    if (Value1.ResultType != null)
                    {
                        var comparable2 = Convert.ChangeType(o, Value1.ResultType) as IComparable;
                        if (comparable1 != null && comparable1.CompareTo(comparable2) == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public Type ArrayType
        {
            get
            {
                if (Value1.ResultType == null || Value1.ResultType == typeof(void))
                {
                    //return Array.CreateInstance(typeof(object), 0).GetType();
                    return null;
                }
                return Array.CreateInstance(Value1.ResultType, 0).GetType();
            }
        }

        public override Type ResultType
        {
            get { return typeof(bool); }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} [{2}]", Value1 == null ? string.Empty : Value1.ToStringAsSubterm(), Symbol, objArrayToStr(Value2 as ICollection));
        }


        public void ValidateList()
        {
            if (this.Value2 != null && !(this.Value2 is ActivityResultTerm))
            {
                if (this.Value1.ResultType != this.Value2.GetType().GetElementType())
                {
                    this.Value2 = null;
                }
            }
            NotifyPropertyChanged("ArrayType");
        }

        private static string objArrayToStr(ICollection arr)
        {
            if (arr == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (object o in arr)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(o);
            }

            return sb.ToString();
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.LogicalTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get
            {
                return TermGroups.NumberTerm
                    | TermGroups.LogicalTerm;
            }
        }
    }
}
