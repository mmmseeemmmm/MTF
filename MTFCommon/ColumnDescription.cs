using System;
using System.Collections.Generic;

namespace AutomotiveLighting.MTFCommon
{
    [Serializable]
    public class ColumnDescription
    {
        public string Name { get; set; }
        public ColumnDataType DataType { get; set; }
        public bool ReadOnly { get; set; }
        public List<string> ListBoxItems { get; set; }
    }

    [Serializable]
    public enum ColumnDataType
    {
        Text,
        Checkbox,
        ListBox,
    }
}
