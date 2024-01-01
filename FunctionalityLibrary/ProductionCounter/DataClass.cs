using System;
using System.Collections.Generic;

namespace ProductionCounter
{
    [Serializable]
    public class DataClass
    {
        private List<DataItem> data = new List<DataItem>();
        private string actualVariant;
        private int targetCountdown;
        private DateTime lastDate;

        public int TargetCountdown
        {
            get { return targetCountdown; }
            set { targetCountdown = value; }
        }

        public string ActualVariant
        {
            get { return actualVariant; }
            set { actualVariant = value; }
        }

        public List<DataItem> Data
        {
            get { return data; }
            set { data = value; }
        }

        public DateTime LastDate
        {
            get { return lastDate; }
            set { lastDate = value; }
        }
    }
}
