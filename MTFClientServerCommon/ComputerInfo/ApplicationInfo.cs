using System.Runtime.CompilerServices;

namespace MTFClientServerCommon.ComputerInfo
{
    public class ApplicationInfo : System.ComponentModel.INotifyPropertyChanged
    {
        private string baseDirectory;
        public string BaseDirectory
        {
            get => baseDirectory;
            set => baseDirectory = value;
        }

        private float memoryUsage;
        public float MemoryUsage
        {
            get => memoryUsage;
            set 
            {
                memoryUsage = value;
                NotifyPropertyChanged();
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
