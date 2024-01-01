using System.Collections.Generic;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor
{
    public class DataPackageWrapper: NotifyPropertyBase
    {
        private bool isCollapsed = true;
        private bool isVisible = true;

        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyPropertyChanged();
            }
        }

        public List<DataWrapper> DataPackage { get; set; }
    }

    public class DataWrapper: NotifyPropertyBase
    {
        private bool isVisible = true;

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyPropertyChanged();
            }
        }

        public ClassInfoData Data { get; set; }
    }
}
