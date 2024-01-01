using System;
using AutomotiveLighting.MTFCommon;

namespace MTFApp.UIHelpers
{
    public class ToggleCommand : Command
    {
        #region Properties

        public bool IsToggle
        {
            get { return true; }
        }

        public string IsCheckedPropertyName { get; private set; }

        #endregion Properties

        public ToggleCommand(Action<object> execute, Func<bool> canExecute, MTFIcons icon, string isCheckedPropertyName)
            : base(execute, canExecute, icon)
        {
            this.IsCheckedPropertyName = isCheckedPropertyName;
        }

        public ToggleCommand(Action<object> execute, Func<bool> canExecute, string isCheckedPropertyName)
            : this(execute, canExecute, MTFIcons.None, isCheckedPropertyName)
        {
        }

        public ToggleCommand(Action execute, Func<bool> canExecute, string isCheckedPropertyName)
            : this(p => execute.Invoke(), canExecute, isCheckedPropertyName)
        {
        }

        public ToggleCommand(Action execute, Func<bool> canExecute, MTFIcons icon, string isCheckedPropertyName)
            : this(p => execute.Invoke(), canExecute, icon, isCheckedPropertyName)
        {
        }

        public ToggleCommand(Action execute, string isCheckedPropertyName)
            : this(execute, () => true, isCheckedPropertyName)
        {
        }

        public ToggleCommand(Action<object> execute, string isCheckedPropertyName)
            : this(execute, () => true, isCheckedPropertyName)
        {
        }
    }
}
