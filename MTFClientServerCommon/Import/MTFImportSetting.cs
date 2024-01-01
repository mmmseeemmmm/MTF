using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.Import
{
    [Serializable]
    public class MTFImportSetting
    {
        public MTFImportSetting()
        {
            Sequences = new List<ImportConflict>();
            ClassInstances = new List<ImportConflict>();
            ImagesSetting = new List<ImportConflict>();
        }

        public List<ImportConflict> Sequences { get; set; }
        public List<ImportConflict> ClassInstances { get; set; }
        public List<ImportConflict> ImagesSetting { get; set; }
    }
}