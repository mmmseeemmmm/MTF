using MTFApp.UIHelpers;

namespace MTFApp.SequenceExecution.ActivityProgress
{
    class ActivityProgressBase : NotifyPropertyBase
    {
        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        private string title;

        public string Title
        {
            get => title;
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }
    }
}