using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.SharedControls
{
    public class VariablesWatch : NotifyPropertyBase
    {
        private MTFVariable variable;

        public MTFVariable Variable
        {
            get => variable;
            set => variable = value;
        }

        private bool isInWatch;

        public bool IsInWatch
        {
            get => isInWatch;
            set
            {
                isInWatch = value;
                NotifyPropertyChanged();
            }
        }

        private object value;

        public object Value
        {
            get => value;
            set
            {
                this.value = value;
                NotifyPropertyChanged();
            }
        }

        private bool isChanged;

        public bool IsChanged
        {
            get => isChanged;
            set
            {
                isChanged = value;
                NotifyPropertyChanged();
            }
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                NotifyPropertyChanged();
            }
        }


        public VariablesWatch(MTFVariable variable)
        {
            this.variable = variable;
            isInWatch = true;
        }
    }
}