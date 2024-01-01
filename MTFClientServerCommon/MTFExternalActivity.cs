using MTFClientServerCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFExternalActivity : MTFCallActivityBase
    {
        public MTFExternalActivity()
            : base()
        {

        }

        public MTFExternalActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public string ExternalSequenceToCall
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string InnerSubSequenceByCall
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                //if (value == null)
                //{
                //    this.ActivityName = "Call <null>";
                //}
                //else if (value == MTFSequenceActivityHelper.CallWholeSequence)
                //{
                //    this.ActivityName = string.Format("Call sequence <{0}>", ExternalSequenceToCall);
                //}
                //else
                //{
                //    this.ActivityName = string.Format("Call <{0}>", value);
                //}
            }
        }

        public List<string> AvailableSubSequences
        {
            get { return GetProperty<List<string>>(); }
            set { SetProperty(value); }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get
            {
                return AutomotiveLighting.MTFCommon.MTFIcons.Execute;
            }
        }
    }
}
