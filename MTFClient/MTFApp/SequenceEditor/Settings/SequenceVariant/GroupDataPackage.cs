using System.Collections.Generic;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class GroupDataPackage
    {
        public string Name { get; set; }

        public List<VariantValueSetting> VariantSetting { get; set; }
    }
}