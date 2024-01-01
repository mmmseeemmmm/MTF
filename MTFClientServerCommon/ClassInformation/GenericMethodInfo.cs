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
    public class GenericMethodInfo : MTFDataTransferObject
    {
        public GenericMethodInfo()
            : base()
        {
        }

        public GenericMethodInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericMethodInfo(MethodInfo methodInfo)
        {
            Name = methodInfo.Name;
            ReturnType = methodInfo.ReturnType.FullName;

            FillParameters(methodInfo);
        }

        protected virtual void FillParameters(MethodInfo methodInfo)
        {
            Parameters = new MTFObservableCollection<GenericParameterInfo>(methodInfo.GetParameters().Select(param => new GenericParameterInfo(param)));
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReturnType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<GenericParameterInfo> Parameters
        {
            get { return GetProperty<IList<GenericParameterInfo>>(); }
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
