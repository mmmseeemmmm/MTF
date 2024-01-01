using System;
using System.Xml.Serialization;

namespace MTFCommon
{
    [Serializable]
    public class MTFLogHeader : ICloneable
    {
        private string sequenceName;

        public string SequenceName
        {
            get { return sequenceName; }
            set { sequenceName = value; }
        }

        private string machine;

        public string Machine
        {
            get { return machine; }
            set { machine = value; }
        }

        private string winUser;

        public string WinUser
        {
            get { return winUser; }
            set { winUser = value; }
        }

        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        private DateTime stopTime;

        public DateTime StopTime
        {
            get { return stopTime; }
            set { stopTime = value; }
        }

        private string duration;

        public string Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        private string sequenceStatus;

        public string SequenceStatus
        {
            get { return sequenceStatus; }
            set { sequenceStatus = value; }
        }

        private bool? sequenceStatusbool;

        [XmlIgnore]
        public bool? SequenceStatusBool
        {
            get => sequenceStatusbool;
            set => sequenceStatusbool = value;
        }

        private string sequenceVariant;

        public string SequenceVariant
        {
            get { return sequenceVariant; }
            set { sequenceVariant = value; }
        }

        [XmlIgnore]
        public string VariantVersion { get; set; }
        [XmlIgnore]   
        public string VariantLD { get; set; }
        [XmlIgnore]   
        public string VariantMS { get; set; }
        [XmlIgnore]   
        public string VariantGsDut { get; set; }

        private string gsRemains;

        public string GsRemains
        {
            get { return gsRemains; }
            set { gsRemains = value; }
        }

        private bool gsWarning;

        public bool GsWarning
        {
            get { return gsWarning; }
            set { gsWarning = value; }
        }

        private string zipDestination;

        public string ZipDestination
        {
            get { return zipDestination; }
            set { zipDestination = value; }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
