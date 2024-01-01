using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFLoggingActivity : MTFSequenceActivity
    {
        public MTFLoggingActivity()
            : base()
        {
        }

        public MTFLoggingActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion
        {
            get
            {
                return "0.0.0";
            }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                Term t = null;
                if (properties.ContainsKey("LogMessage"))
                {
                    t = properties["LogMessage"] as Term;
                    if (t != null)
                    {
                        MTFStringFormat sf = new MTFStringFormat();
                        sf.Text = "{0}";
                        sf.Parameters = new MTFObservableCollection<Mathematics.Term>();
                        sf.Parameters.Add(t);

                        LogMessage = sf;
                    }
                }

                if (properties.ContainsKey("LogFileName"))
                {
                    t = properties["LogFileName"] as Term;
                    if (t != null)
                    {
                        MTFStringFormat sf = new MTFStringFormat();
                        sf.Text = "{0}";
                        sf.Parameters = new MTFObservableCollection<Mathematics.Term>();
                        sf.Parameters.Add(t);

                        LogFileName = sf;
                    }
                }
                fromVersion = "1.1.0";
            }
            if (fromVersion == "1.1.0")
            {
                //added log time stamp -> default should be true
                fromVersion = "1.2.0";
                LogTimeStamp = true;
            }
        }

        public MTFStringFormat LogMessage
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }


        public MTFStringFormat LogFileName
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public bool LogTimeStamp
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public LoggingType LoggingType
        {
            get { return GetProperty<LoggingType>(); }
            set
            {
                SetProperty(value);
                this.ActivityName = value.Description(); ;
            }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get
            {
                return AutomotiveLighting.MTFCommon.MTFIcons.Pencil;
            }
        }
    }


    public enum LoggingType
    {
        [Description("Log message")]
        LogMessage,
        [Description("Open Logfile")]
        OpenLogFile,
    }
}
