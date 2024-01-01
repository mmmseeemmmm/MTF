using System;
using AutomotiveLighting.MTFCommon;

namespace ProductionCounter
{
    [MTFKnownClass]
    public class SerialNumber
    {
        private string date;
        private string time;
        private string runningNumber;

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public string Time
        {
            get { return time; }
            set { time = value; }
        }

        public string RunningNumber
        {
            get { return runningNumber; }
            set { runningNumber = value; }
        }
    }
}