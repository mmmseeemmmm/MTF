using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class ExternalCallInfo : MTFDataTransferObject
    {
        [field: NonSerialized]
        private List<ExternalActivityInfo> availableSubSequences;

        [field: NonSerialized]
        private ExternalActivityInfo selectedValue;

        public ExternalCallInfo()
            : base()
        {
        }

        public ExternalCallInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        public string ExternalSequenceToCall
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Guid InnerSubSequenceByCallId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        public void InvalidateExternalSubSequenceToCall()
        {
            NotifyPropertyChanged("InnerSubSequenceByCallId");
        }

        public List<ExternalActivityInfo> AvailableSubSequences
        {
            get { return availableSubSequences; }
            set
            {
                availableSubSequences = value;
                NotifyPropertyChanged();
            }
        }

        public ExternalActivityInfo SelectedValue
        {
            get { return selectedValue; }
            set
            {
                selectedValue = value;
                if (value != null)
                {
                    OriginalCallActivityName = value.OriginalName;
                    CallActivityIndexer = value.UniqueIndexer;
                }
            }
        }

        public string OriginalCallActivityName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int CallActivityIndexer
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

    }

    public class ExternalActivityInfo
    {
        public Guid Id { get; set; }

        public string TranslatedName { get; set; }

        public int UniqueIndexer { get; set; }

        public string OriginalName { get; set; }
    }
}