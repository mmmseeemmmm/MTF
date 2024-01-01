using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFVariableActivity : MTFSequenceActivity
    {
        public MTFVariableActivity()
            : base()
        {

        }

        public MTFVariableActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        //[MTFPersistIdOnly]
        //public MTFVariable Variable
        //{
        //    get { return GetProperty<MTFVariable>(); }
        //    set { SetProperty(value); }
        //}

        public Term Value
        {
            get { return GetProperty<Term>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged();
            }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get
            {
                return AutomotiveLighting.MTFCommon.MTFIcons.Variable;
            }
        }
    }
}
