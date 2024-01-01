using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTFClientServerCommon.Import
{
    [Serializable]
    public class ImportConflict : INotifyPropertyChanged
    {
        private string fullFileName;
        private Guid id;
        private string conflictItem;
        private string subName;
        private bool isConflict;
        private bool overrideOriginal;
        private bool enableImport;
        private string storePath;

        public string FullFileName
        {
            get => fullFileName;
            set => fullFileName = value;
        }

        public string FileName => System.IO.Path.GetFileNameWithoutExtension(fullFileName);


        public string SubName
        {
            get => subName;
            set => subName = value;
        }


        public bool IsConflict
        {
            get => isConflict;
            set
            {
                isConflict = value;
                NotifyPropertyChanged();
            }
        }


        public bool OverrideOriginal
        {
            get => overrideOriginal;
            set
            {
                overrideOriginal = value;
                NotifyPropertyChanged();
            }
        }


        public bool EnableImport
        {
            get => enableImport;
            set
            {
                enableImport = value;
                NotifyPropertyChanged();
            }
        }


        public string StorePath
        {
            get => storePath;
            set => storePath = value;
        }

        public Guid Id
        {
            get => id;
            set => id = value;
        }

        public string ConflictItem
        {
            get => conflictItem;
            set => conflictItem = value;
        }


        public ImportConflict(Guid id, string fileName, bool isConflict, bool overrideOriginal)
            : this(id, fileName, string.Empty, isConflict, overrideOriginal)
        {
        }

        public ImportConflict(Guid id, string fileName, string subName, bool isConflict, bool overrideOriginal)
        {
            this.id = id;
            this.enableImport = true;
            this.subName = subName;
            this.fullFileName = fileName;
            this.isConflict = isConflict;
            this.overrideOriginal = overrideOriginal;
        }

        [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}