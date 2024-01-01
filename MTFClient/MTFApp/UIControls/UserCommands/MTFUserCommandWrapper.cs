using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.UIControls.UserCommands
{
    public class MTFUserCommandWrapper : NotifyPropertyBase
    {
        private MTFUserCommand command;
        private bool isEnabled;
        private bool isChecked;
        private bool indicatorValue;
        public MTFUserCommand Command
        {
            get => command;
            set
            {
                command = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get => isChecked; 
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IndicatorValue
        {
            get => indicatorValue; 
            set
            {
                indicatorValue = value;
                NotifyPropertyChanged();
            }
        }
    }
}
