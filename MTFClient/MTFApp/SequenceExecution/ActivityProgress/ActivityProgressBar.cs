namespace MTFApp.SequenceExecution.ActivityProgress
{
    class ActivityProgressBar : ActivityProgressBase
    {
        private int percent;
        private string text;
        private bool isIndeterminate;

        public int Percent {
            get { return percent; }
            set
            {
                percent = value;
                NotifyPropertyChanged();
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                isIndeterminate = value;
                NotifyPropertyChanged();
            }
        }
    }
}
