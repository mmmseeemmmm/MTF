using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.OpenSaveSequencesDialog
{
    public class SequenceFileContainer : NotifyPropertyBase
    {
        private bool isEditable = false;
        private string name;

        private string fullName;

        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                NotifyPropertyChanged();
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public MTFDialogItemTypes SequenceFileType { get; set; }

        public bool IsEditable
        {
            get => isEditable;
            set
            {
                isEditable = value;
                NotifyPropertyChanged();
            }
        }

        public bool CanChangeLocation => SequenceFileType != MTFDialogItemTypes.File;

        public bool CanBrowse => SequenceFileType != MTFDialogItemTypes.Up && CanChangeLocation;


        public string DisplayShortName
        {
            get
            {
                //if (Name.Contains('.'))
                //{
                //    var disp = Name.Remove(Name.LastIndexOf('.'));
                if (Name.Length > 30)
                {
                    return Name.Substring(0, 30) + "...";
                }
                return Name;
                //}
                //else
                //{
                //    return Name;
                //}
            }
        }

        public SequenceFileContainer(string name, string fullName, MTFDialogItemTypes sequenceFileType)
        {
            this.Name = name;
            this.FullName = fullName;
            this.IsEditable = false;
            this.SequenceFileType = sequenceFileType;
        }

        public SequenceFileContainer(MTFDialogItemTypes sequenceFileType)
            : this(string.Empty, string.Empty, sequenceFileType)
        {
        }
    }


}
