using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.Helpers
{
    public class ExecutionPointerHelper
    {
        private MTFActivityVisualisationWrapper currentPointer;
        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                if (value)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        private void Show()
        {
            if (currentPointer!=null)
            {
                currentPointer.IsExecutionPointer = true;
            }
        }

        public void Hide()
        {
            if (currentPointer != null)
            {
                currentPointer.IsExecutionPointer = false;
            }
        }

        public void Change(MTFActivityVisualisationWrapper newPointer)
        {
            if (IsActive)
            {
                Hide();
                currentPointer = newPointer;
                Show();
            }
            else
            {
                currentPointer = newPointer;
            }
        }
    }
}
