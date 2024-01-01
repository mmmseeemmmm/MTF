using System;
using System.Windows;
using AutomotiveLighting.MTFCommon;

namespace MTFCommon.ClientControls
{
    public class MTFClientControl : MTFClientControlBase
    {
        public MTFClientControl()
        {
            IsEnabled = false;
        }

        public virtual void OnActivityChanged(MTFActivity activity)
        {
        }

        public virtual void OnNewActivityResult(MTFActivityResult activityResult)
        {
        }

        public virtual void OnErrorMessage(DateTime timeStamp, string activityName, string text, MessageType type)
        {
        }

        public virtual void OnExecutionStateChanged(SequenceExecutionState newState)
        {
            Application.Current.Dispatcher.Invoke(
                () => IsEnabled = newState == SequenceExecutionState.Executing || newState == SequenceExecutionState.Pause);
        }

        public virtual void OnStatusMessage(string line1, string line2, string line3, int fontSize1, int fontSize2, int fontSize3)
        {
        }

        public virtual void OnVariantChanged(MTFSequenceVariant variant)
        {
        }

        public virtual bool AllowMTFHeader { get { return false; } }
        public virtual bool AllowMTFErrorWindow { get { return false; } }
        public virtual bool AllowMTFGoldSampleWatch { get { return false; } }
    }
}
