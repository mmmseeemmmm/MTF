using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFListOperation : MTFDataTransferObject
    {
        public MTFListOperation()
            : base()
        {
        }

        public MTFListOperation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        } 

        public IList<Term> Parameters
        {
            get { return GetProperty<IList<Term>>(); }
            set { SetProperty(value); }
        }
    }
}
