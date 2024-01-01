using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MTFClientServerCommon.Helpers
{
    class Command : ICommand
    {
        private Action<object> execute;
        private Func<bool> canExecute;
        public string Name { get; set; }

        public Command(Action<object> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public Command(Action execute, Func<bool> canExecute)
            : this((p) => execute.Invoke(), canExecute)
        {
        }

        public Command(Action execute)
            : this(execute, () => true)
        {
        }

        public Command(Action<object> execute)
            : this(execute, () => true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            execute.Invoke(parameter);
        }
    }
}
