using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTFClientServerCommon.GraphicalView
{
    [Serializable]
    public class GraphicalViewImg : INotifyPropertyChanged
    {
        private string fileName;
        private string name;
        private byte[] data;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}