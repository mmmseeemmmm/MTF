using System;

namespace ProductionCounter
{
    [Serializable]
    public class DataItem
    {
        private string variant;
        private int count;

        public string Variant
        {
            get { return variant; }
            set { variant = value; }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }
    }
}
