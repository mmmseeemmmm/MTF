using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericPropertyIndexer: GenericPropertyInfo
    {
        public GenericPropertyIndexer()
            :base()
        {
        }

        public GenericPropertyIndexer(GenericPropertyInfo gpi)
            : base()
        {
            this.AllowedValues = gpi.AllowedValues;
            this.CanRead = gpi.CanRead;
            this.CanWrite = gpi.CanWrite;
            this.Id = gpi.Id;
            this.Name = gpi.Name;
            this.Type = gpi.Type;
        }

        public GenericPropertyIndexer(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public object Index
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }
    }
}
