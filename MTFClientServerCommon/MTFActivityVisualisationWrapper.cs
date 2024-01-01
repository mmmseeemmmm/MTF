using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTFClientServerCommon
{
    public class MTFActivityVisualisationWrapper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Nesting { get; set; }

        public bool IsDynamicActivity { get; set; }

        public bool IsDynamicRoot { get; set; }

        private MTFActivityResult result;

        public MTFActivityResult Result
        {
            get { return result; }
            set
            {
                result = value;
                NotifyPropertyChanged();
            }
        }

        private bool isInTree;

        public bool IsInTree
        {
            get { return isInTree; }
            set
            {
                isInTree = value;
                NotifyPropertyChanged();
            }
        }

        private bool canInsert = true;

        public bool CanInsert
        {
            get { return canInsert; }
            set
            {
                canInsert = value;
            }
        }

        private StateDebugSetup isSetupMode;
        public StateDebugSetup IsSetupMode
        {
            get { return isSetupMode; }
            set 
            {
                isSetupMode = value;
                NotifyPropertyChanged();
            }
        }

        private StateDebugSetup isBreakPoint;
        public StateDebugSetup IsBreakPoint
        {
            get { return isBreakPoint; }
            set 
            { 
                isBreakPoint = value;
                NotifyPropertyChanged();
            }
        }

        public MTFSequenceActivity Activity { get; set; }

        private MTFExecutionActivityStatus status;

        public MTFExecutionActivityStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        private bool isExecucitonPointer;
        public bool IsExecutionPointer
        {
            get { return isExecucitonPointer; }
            set
            {
                isExecucitonPointer = value;
                NotifyPropertyChanged();
            }
        }

        private List<Guid> guidPath = new List<Guid>();

        public List<Guid> GuidPath
        {
            get { return guidPath; }
            set { guidPath = value; }
        }

        public bool HasSetup { get; set; }

        public bool IsExecuteAsOneActivity { get; set; }

        public bool IsExpandable { get; set; }

        private bool isCollapsed;

        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value; 
                NotifyPropertyChanged();
            }
        }



        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    
}
