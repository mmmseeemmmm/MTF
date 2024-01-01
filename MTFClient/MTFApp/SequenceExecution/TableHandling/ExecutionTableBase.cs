using System;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceExecution.TableHandling
{
    public abstract class ExecutionTableBase : NotifyPropertyBase
    {
        private bool isCollapsed;
        private bool isEditable;


        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsEditable
        {
            get { return isEditable; }
            set
            {
                isEditable = value;
                NotifyPropertyChanged();
            }
        }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public abstract void Clear();
    }
}