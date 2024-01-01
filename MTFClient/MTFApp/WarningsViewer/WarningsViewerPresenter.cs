using System.Collections.ObjectModel;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.WarningsViewer
{
    public class WarningsViewerPresenter : PresenterBase
    {
        private readonly ICommand removeWarningCommand;
        private readonly ICommand clearWarningsCommand;
        public WarningsViewerPresenter()
        {
            removeWarningCommand = new Command(removeCommand);
            clearWarningsCommand = new Command(clearWarnings);
        }

        public ICommand RemoveWarningCommand
        {
            get { return removeWarningCommand; }
        }

        public ICommand ClearWarningsCommand
        {
            get { return clearWarningsCommand; }
        }

        public ObservableCollection<WarningMessage> Warnings
        {
            get { return Warning.Messages; }
        }

        private void removeCommand(object param)
        {
            Warning.Remove(((WarningMessage)param).Id);
        }

        private void clearWarnings()
        {
            Warning.Clear();
        }
    }
}
