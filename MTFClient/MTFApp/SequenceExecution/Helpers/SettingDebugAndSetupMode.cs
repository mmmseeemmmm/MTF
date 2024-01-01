using System;
using System.Collections.Generic;

namespace MTFApp.SequenceExecution.Helpers
{
    [Serializable]
    public class SettingDebugAndSetupMode
    {
        private Guid sequenceId;
        private List<ExtendedModeActivitySetting> listActivities;
        private bool active;

        public Guid SequenceId
        {
            get { return sequenceId; }
            set { sequenceId = value; }
        }

        public List<ExtendedModeActivitySetting> ListActivities
        {
            get { return listActivities; }
            set { listActivities = value; }
        }

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public bool IsEmpty
        {
            get { return listActivities == null || listActivities.Count == 0; }
        }
    }
}
