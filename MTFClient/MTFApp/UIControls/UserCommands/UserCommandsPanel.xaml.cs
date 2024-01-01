using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;

namespace MTFApp.UIControls.UserCommands
{
    /// <summary>
    /// Interaction logic for UserCommands.xaml
    /// </summary>
    public partial class UserCommandsPanel : UserControl
    {
        public UserCommandsPanel()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public IEnumerable<MTFUserCommandWrapper> UserCommands
        {
            get => (IEnumerable<MTFUserCommandWrapper>)GetValue(UserCommandsProperty);
            set => SetValue(UserCommandsProperty, value);
        }

        public static readonly DependencyProperty UserCommandsProperty = DependencyProperty.Register("UserCommands", typeof(IEnumerable<MTFUserCommandWrapper>), typeof(UserCommandsPanel));

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var commandWrapper = (MTFUserCommandWrapper)button?.DataContext;
            if (commandWrapper == null)
            {
                return;
            }

            foreach (var command in UserCommands.Where(c => c.Command.Type == MTFUserCommandType.Button || c.Command.Type == MTFUserCommandType.ToggleButton))
            {
                command.IsEnabled = false;
            }

            MTFClient.GetMTFClient().ExecuteUserCommand(commandWrapper.Command.Id);
        }
    }
}
