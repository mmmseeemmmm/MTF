using System;
using AutomotiveLighting.MTFCommon;

namespace AssemblyWorkStationCommon
{
    [Serializable]
    [MTFKnownClass]
    public class StepItem
    {
        private  readonly Guid id;

        public StepItem()
        {
            id = Guid.NewGuid();
        }

        public string StepName { get; set; }
        public MTFActivity Activity { get; set; }

        public Guid GetId()
        {
            return id;
        }
    }
}
