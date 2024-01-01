using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTFClientServerCommon
{
    [Serializable]
    public class ServerSettings : INotifyPropertyChanged
    {
        private string mtfServerAddress = "localhost";
        private string mtfServerPort = "2442";
        private string mtfServerReportingPort = "2443";
        private bool startSequenceOnServerStart;
        private string sequenceName;
        private bool allowXmlLogging;
        private ErrorMessageType errorMessageType;
        private bool pauseAfterError;
        private string language = "en-US";

        public string Language
        {
            get => language;
            set => language = value;
        }

        public string MTFServerAddress
        {
            get => mtfServerAddress;
            set => mtfServerAddress = value;
        }

        public string MTFServerPort
        {
            get => mtfServerPort;
            set => mtfServerPort = value;
        }

        public string MTFServerReportingPort
        {
            get => mtfServerReportingPort;
            set => mtfServerReportingPort = value;
        }

        public bool StartSequenceOnServerStart
        {
            get => startSequenceOnServerStart;
            set
            {
                startSequenceOnServerStart = value;
                NotifyPropertyChanged();
            }
        }

        public string SequenceName
        {
            get => sequenceName;
            set
            {
                sequenceName = value;
                NotifyPropertyChanged();
            }
        }

        public bool AllowXmlLogging
        {
            get => allowXmlLogging;
            set
            {
                allowXmlLogging = value;
                NotifyPropertyChanged();
            }
        }

        public ErrorMessageType ErrorMessageType
        {
            get => errorMessageType;
            set => errorMessageType = value;
        }

        public bool PauseAfterError
        {
            get => pauseAfterError;
            set => pauseAfterError = value;
        }


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum ErrorMessageType
    {
        [Description("Enum_ErrorMsgType_None")]
        None,
        [Description("Enum_ErrorMsgType_NonBlocking")]
        NonBlockingWindow,
        [Description("Enum_ErrorMsgType_Blocking")]
        BlockingWindow
    }
}
