using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MTFCommon
{
    [Serializable]
    [XmlRoot("Overview")]
    public class MTFLogOverview
    {
        private MTFLogHeader header;

        public MTFLogHeader Header
        {
            get { return header; }
            set { header = value; }
        }

        private List<MTFLogOverviewRow> rows;
        [XmlArray("Rows")]
        [XmlArrayItem("Row")]
        public List<MTFLogOverviewRow> Rows
        {
            get { return rows; }
            set { rows = value; }
        }

        public void SetStopTime()
        {
            if (Header!=null)
            {
                Header.StopTime = DateTime.Now;
                Header.Duration = (Header.StopTime - Header.StartTime).ToString();
            }
        }
        
    }

    [Serializable]
    public class MTFLogOverviewRow
    {
        private string logPath;

        public string LogPath
        {
            get { return logPath; }
            set { logPath = value; }
        }

        private string logName;

        public string LogName
        {
            get { return logName; }
            set { logName = value; }
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

        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        private string zipDestination;

        public string ZipDestination
        {
            get { return zipDestination; }
            set { zipDestination = value; }
        }
    }
}
