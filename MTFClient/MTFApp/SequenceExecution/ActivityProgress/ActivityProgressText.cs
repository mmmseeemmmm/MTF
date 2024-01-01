namespace MTFApp.SequenceExecution.ActivityProgress
{
    class ActivityProgressText : ActivityProgressBase
    {
        private string textValue;
        public string TextValue 
        {
            get { return textValue; }
            set
            {
                textValue = value;
                NotifyPropertyChanged();
            }
        }
    }
}
