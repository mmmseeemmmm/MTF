using System;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ActivityTargetTerm : Term
    {
        public ActivityTargetTerm()
            : base()
        {

        }

        public ActivityTargetTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity Value
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set { SetProperty(value, false); }
        }

        public override object Evaluate()
        {
            if (Value!=null)
            {
                return new MTFActivity
                       {
                           Id = Value.Id,
                           ActivityName = Value.ActivityName,
                           ClassAlias = Value.MTFClassAlias,
                           MethodName = Value.MTFMethodName,
                       };
            }
            return null;
        }

        public override Type ResultType
        {
            get { return typeof(MTFActivity); }
        }

        public override string Symbol
        {
            get { return string.Empty; }
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
            get { return MTFIcons.None; }
        }

        public override string Label
        {
            get { return string.Empty; }
        }
    }
}
