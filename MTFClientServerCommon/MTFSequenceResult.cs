using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFSequenceResult : MTFPersist
    {
        public MTFSequenceResult()
            : base()
        {
        }

        public MTFSequenceResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        //if setup mode is used -> sequence was modified by executor
        public bool SequenceChangedByExecution
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SequenceName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Guid SequenceId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        public DateTime ExecutionStart
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime ExecutionStop
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public TimeSpan ExecutionDuration
        {
            get
            {
                if (ExecutionStart!=null && ExecutionStop!=null)
                {
                    var durationTime = ExecutionStop - ExecutionStart;

                    return new TimeSpan(durationTime.Days, durationTime.Hours, durationTime.Minutes, durationTime.Seconds, (int)(Math.Truncate((decimal)durationTime.Milliseconds / 10) * 10));
                }
                return TimeSpan.Zero;
            }
        }

        public IList<MTFActivityResult> ActivityResults
        {
            get { return GetProperty<IList<MTFActivityResult>>(); }
            set { SetProperty(value); }
        }
    }
}
