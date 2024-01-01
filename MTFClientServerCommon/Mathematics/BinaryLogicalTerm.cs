using AutomotiveLighting.MTFCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public abstract class BinaryLogicalTerm : BinaryTerm
    {
        public BinaryLogicalTerm()
            : base()
        {
        }

        public BinaryLogicalTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override Type ResultType
        {
            get { return typeof(bool); }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.LogicalTerm;
            }
        }

        protected bool areEqualValues(object val1, object val2)
        {
            if (val1 == null && val2 == null)
            {
                return true;
            }

            if(val1 == null || val2 == null)
            {
                return false;
            }

            if (val1 is Enum || val2 is Enum)
            {
                return val1.ToString() == val2.ToString();
            }

            Helpers.TypeInfo typeInfo = new Helpers.TypeInfo(val1.GetType().FullName);
            if (typeInfo.IsSimpleType)
            {
                return areEqualSimpleTypes(val1, val2);
            }

            if (!val1.GetType().Equals(val2.GetType()))
            {
                return false;
            }

            //val1 and val2 has same type -> it make sence to compare it
            if (typeInfo.IsCollection)
            {
                return areEqualICollection((ICollection)val1, (ICollection)val2);
            }

            return areEqualMTFKnownObjects(val1, val2);
        }

        private bool areEqualMTFKnownObjects(object val1, object val2)
        {
            if (!val1.GetType().GetCustomAttributes(typeof(MTFKnownClassAttribute), true).Any())
            {
                return false;
            }

            foreach (var prop in val1.GetType().GetProperties())
            {
                if (!areEqualValues(prop.GetValue(val1), prop.GetValue(val2)))
                {
                    return false;
                }
            }

            return true;
        }

        private bool areEqualSimpleTypes(object val1, object val2)
        {
            var comparable1 = val1 as IComparable;
            var comparable2 = Convert.ChangeType(val2, val1.GetType()) as IComparable;

            if ((comparable1 == null && comparable2 != null) || (comparable1 != null && comparable2 == null))
            {
                return false;
            }

            if (comparable1 == null && comparable2 == null)
            {
                return false;
            }

            if (comparable1 == null || comparable2 == null)
            {
                throw new Exception("Can't compare values.");
            }

            return comparable1.CompareTo(comparable2) == 0;
        }

        private bool areEqualICollection(ICollection val1, ICollection val2)
        {
            if (val1.Count != val2.Count)
            {
                return false;
            }

            var enumerator1 = val1.GetEnumerator();
            var enumerator2 = val2.GetEnumerator();

            while (enumerator1.MoveNext())
            {
                enumerator2.MoveNext();
                if (!areEqualValues(enumerator1.Current, enumerator2.Current))
                {
                    return false;
                }
            }

            return true;
        }
    }
}