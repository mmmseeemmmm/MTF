using System;
using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.ExtendedMode
{
    public class ExtendedModeActions
    {
        public Action<MTFActivityVisualisationWrapper> SelectItemAction { get; set; }
        public Action<MTFActivityVisualisationWrapper> ChangeBreakPointAction { get; set; }
        public Action<MTFActivityVisualisationWrapper> ChangeSetupPointAction { get; set; }
    }
}