using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MTFApp.UIHelpers
{
    public class Command : NotifyPropertyBase, ICommand
    {
        private Action<object> execute;
        private Func<bool> canExecute;
        private bool showKeyShortcut;
        private bool focus;

        public Command(Action<object> execute, Func<bool> canExecute, MTFIcons icon)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.Icon = icon;
        }
        
        public Command(Action<object> execute, Func<bool> canExecute)
            : this(execute, canExecute, MTFIcons.None)
        {
        }

        public Command (Action execute, Func<bool> canExecute)
            : this((p)=>execute.Invoke(), canExecute)
        {
        }

        public Command(Action execute, Func<bool> canExecute, MTFIcons icon)
            : this((p) => execute.Invoke(), canExecute, icon)
        {
        }

        public string Name { get; set; }
        
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
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            execute.Invoke(parameter);
        }

        public MTFIcons Icon
        {
            get;
            set;
        }

        public IEnumerable<CommandShortcut> KeyShortucuts
        {
            get;
            set;
        }

        public bool ShowKeyShortcut
        {
            get => showKeyShortcut;
            set
            {
                showKeyShortcut = value;
                NotifyPropertyChanged("IsShortcutPopupOpen");
            }
        }

        public string ShortcutNames
        {
            get
            {
                if (KeyShortucuts == null)
                {
                    return null;
                }

                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (var ks in KeyShortucuts)
                {
                    if (!first)
                    {
                        sb.AppendLine();
                    }
                    if (AppendModifierKey(ks.Modifier, sb))
                    {
                        sb.Append("+");
                    }
                    sb.Append("<").Append(ks.Key).Append(">");
                    first = false;
                }

                return sb.ToString();
            }
        }

        private bool AppendModifierKey(ModifierKeys modifier, StringBuilder sb)
        {
            bool appendPlus = false;
            if (modifier.HasFlag(ModifierKeys.Control))
            {
                sb.Append("<Ctrl>");
                appendPlus = true;
            }

            if (modifier.HasFlag(ModifierKeys.Alt))
            {
                if (appendPlus)
                {
                    sb.Append("+");
                }
                appendPlus = true;
                sb.Append("<Alt>");
            }

            if (modifier.HasFlag(ModifierKeys.Shift))
            {
                if (appendPlus)
                {
                    sb.Append("+");
                }
                appendPlus = true;
                sb.Append("<Shift>");
            }

            if (modifier.HasFlag(ModifierKeys.Windows))
            {
                if (appendPlus)
                {
                    sb.Append("+");
                }
                appendPlus = true;
                sb.Append("<Win>");
            }
            return appendPlus;
        }

        public bool IsShortcutPopupOpen => KeyShortucuts != null && ShowKeyShortcut;

        public bool Focus
        {
            get => focus;
            set
            {
                focus = value;
                NotifyPropertyChanged();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }

    public class CommandShortcut
    {
        public Key Key { get; set; }
        public ModifierKeys Modifier { get; set; }
    }
}
