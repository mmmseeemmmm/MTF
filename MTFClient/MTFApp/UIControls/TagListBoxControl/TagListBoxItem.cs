using MTFApp.UIHelpers;

namespace MTFApp.UIControls.TagListBoxControl
{
    public class TagListItem : NotifyPropertyBase
    {
        private bool isCLosed = true;
        private string value;

        public string Value
        {
            get => value;

            set
            {
                if (value.Contains("\r\n"))
                {
                    if (value == "\r\n")
                    {
                        this.value = string.Empty;
                    }
                    IsClosed = true;
                }
                else
                {
                    this.value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsClosed
        {
            get => isCLosed;

            set
            {
                isCLosed = value;
                NotifyPropertyChanged();
            }
        }
    }
}
