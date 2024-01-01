using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public abstract class UnaryRoundTerm : UnaryNumberTerm
    {
        public UnaryRoundTerm()
            : base()
        {
            SelectedRoundMode = RoundModes.Round;
        }

        public UnaryRoundTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public int RoundValue
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public RoundModes SelectedRoundMode
        {
            get { return GetProperty<RoundModes>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<EnumValueDescription> ListRoundModes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<RoundModes>(); }
        }
    }
}
