using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.GoldSamplePersist
{
    [Serializable]
    public class GoldSamplePersist
    {
        private List<SequenceVariantInfo> goldSampleList;
        private Dictionary<Guid, Dictionary<Guid, GoldSampleValue>> tableValues;
        private DateTime? lastUsedVariantTime;
        private bool wasGSAfterStart;
        private SequenceVariant lastGoldSample;
        private int otherVariantCounter;


        public GoldSamplePersist()
        {
            goldSampleList = new List<SequenceVariantInfo>();
            tableValues = new Dictionary<Guid, Dictionary<Guid, GoldSampleValue>>();
        }


        public List<SequenceVariantInfo> GoldSampleList
        {
            get { return goldSampleList; }
            set { goldSampleList = value; }
        }

        public void AddGoldSampleVariant(SequenceVariantInfo gs, bool allowMore)
        {
            if (!allowMore)
            {
                goldSampleList.Clear();
            }
            goldSampleList.Add(gs);
        }

        public void AddTableValue(Guid tableId, Guid rowId, GoldSampleValue value)
        {
            if (tableValues.ContainsKey(tableId))
            {
                var rows = tableValues[tableId] ?? new Dictionary<Guid, GoldSampleValue>();
                rows[rowId] = value;
            }
            else
            {
                tableValues.Add(tableId, new Dictionary<Guid, GoldSampleValue> { { rowId, value } });
            }
        }

        public bool IsNotEmpty
        {
            get
            {
                return GoldSampleList != null && GoldSampleList.Count > 0 || tableValues != null && tableValues.Count > 0;
            }
        }

        public Dictionary<Guid, Dictionary<Guid, GoldSampleValue>> TableValues
        {
            get { return tableValues; }
            set { tableValues = value; }
        }

        public DateTime? LastUsedVariantTime
        {
            get { return lastUsedVariantTime; }
            set { lastUsedVariantTime = value; }
        }

        public SequenceVariant LastGoldSample
        {
            get { return lastGoldSample; }
            set { lastGoldSample = value; }
        }

        public int OtherVariantCounter
        {
            get { return otherVariantCounter; }
            set { otherVariantCounter = value; }
        }


        public void CheckNull()
        {
            if (tableValues == null)
            {
                tableValues = new Dictionary<Guid, Dictionary<Guid, GoldSampleValue>>();
            }
            if (goldSampleList == null)
            {
                goldSampleList = new List<SequenceVariantInfo>();
            }
        }
    }
}
