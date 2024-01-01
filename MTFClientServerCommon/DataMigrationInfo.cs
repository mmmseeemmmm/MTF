using System;

namespace MTFClientServerCommon
{
    [Serializable]
    public class DataMigrationInfo
    {
        public string PreviousMTFVersion { get; set; }
        public bool MigrationPossible { get; set; }
    }
}
