using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.UIControls.CommandGrid
{
    public class MTFServiceCommandWrapper : NotifyPropertyBase
    {
        private MTFServiceCommand command;
        private bool isActual;
        private int width;
        private bool isChecked;

        public MTFServiceCommand Command
        {
            get { return command; }
            set
            {
                command = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsActual
        {
            get { return isActual; }
            set
            {
                isActual = value;
                NotifyPropertyChanged();
            }
        }

        public int X { get; private set; }

        public int Y { get; private set; }

        public Command ExecutedCommand { get; set; }

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasCommand
        {
            get { return command != null; }
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }
        }


        public MTFServiceCommandWrapper(MTFServiceCommand command, int x, int y)
        {
            this.command = command;
            X = x;
            Y = y;
        }

        public MTFServiceCommandWrapper(MTFServiceCommand command)
        {
            this.command = command;
        }
    }
}
