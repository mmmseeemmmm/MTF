using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericConstructorInfo : MTFDataTransferObject
    {
        public GenericConstructorInfo()
            : base()
        {
        }

        public GenericConstructorInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericConstructorInfo(ConstructorInfo constructorInfo)
        {
            Name = "Constructor";
            FillParameters(constructorInfo);
        }

        protected virtual void FillParameters(ConstructorInfo constructorInfo)
        {
            Parameters = new MTFObservableCollection<GenericParameterInfo>(constructorInfo.GetParameters().Select(p => new GenericParameterInfo(p)));
        }

        public IList<GenericParameterInfo> Parameters
        {
            get { return GetProperty<IList<GenericParameterInfo>>(); }
            set { SetProperty(value); }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name).Append("(");
            bool firstParam = true;
            foreach (var p in Parameters)
            {
                if (!firstParam)
                {
                    sb.Append(", ");
                }
                else
                {
                    firstParam = false;
                }

                sb.Append(p);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
