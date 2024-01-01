using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor
{
    /// <summary>
    /// Interaction logic for UserCommandSettings.xaml
    /// </summary>
    public partial class UserCommandSettings : UserControl, INotifyPropertyChanged
    {
        private Command selectedUserCommandMoveUpCommand;
        private Command selectedUserCommandMoveDownCommand;

        public UserCommandSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
            initializeCommands();
        }

        private void initializeCommands()
        {
            selectedUserCommandMoveUpCommand = new Command(selectedUserCommandMoveUp);
            selectedUserCommandMoveDownCommand = new Command(selectedUserCommandMoveDown);
        }

        public MTFUserCommand UserCommand
        {
            get => (MTFUserCommand)GetValue(UserCommandProperty); 
            set => SetValue(UserCommandProperty, value); 
        }

        public static readonly DependencyProperty UserCommandProperty = DependencyProperty.Register("UserCommand", typeof(MTFUserCommand), typeof(UserCommandSettings), new FrameworkPropertyMetadata { PropertyChangedCallback = UserCommandChangedCallback });

        private static void UserCommandChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            UserCommandSettings control = dependencyObject as UserCommandSettings;
            if (control != null)
            {
                var newUserCommand = dependencyPropertyChangedEventArgs.NewValue as MTFUserCommand;
                if (newUserCommand != null)
                {
                    control.OnPropertyChanged("ExecuteActivityVisibility");
                    control.OnPropertyChanged("ToggleOffActivityVisibility");
                    newUserCommand.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "Type")
                        {
                            control.OnPropertyChanged("ExecuteActivityVisibility");
                            control.OnPropertyChanged("ToggleOffActivityVisibility");
                        }
                    };
                }
            }
        }

        public MTFObservableCollection<MTFUserCommand> AllUserCommands
        {
            get { return (MTFObservableCollection<MTFUserCommand>)GetValue(AllUserCommandsProperty); }
            set { SetValue(AllUserCommandsProperty, value); }
        }

        public static readonly DependencyProperty AllUserCommandsProperty =
            DependencyProperty.Register("AllUserCommands", typeof(MTFObservableCollection<MTFUserCommand>), typeof(UserCommandSettings), new FrameworkPropertyMetadata { PropertyChangedCallback = UserCommandChangedCallback });


        private void selectedUserCommandMoveUp()
        {
            if (UserCommand != null)
            {
                var index = AllUserCommands.IndexOf(UserCommand);
                if (index > 0)
                {
                    AllUserCommands.Move(index, index - 1);
                }
            }
        }

        private void selectedUserCommandMoveDown()
        {
            if (UserCommand != null)
            {
                var index = AllUserCommands.IndexOf(UserCommand);
                if (index < AllUserCommands.Count - 1)
                {
                    AllUserCommands.Move(index, index + 1);
                }
            }
        }

        public IEnumerable<EnumValueDescription> CommandTypes  => EnumHelper.GetAllValuesAndDescriptions<MTFUserCommandType>();

        public IEnumerable<EnumValueDescription> CommandAccessRoles  => EnumHelper.GetAllValuesAndDescriptions<MTFUserCommandAccessRole>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility ExecuteActivityVisibility => UserCommand != null && (UserCommand.Type == MTFUserCommandType.Button ||  UserCommand.Type == MTFUserCommandType.ToggleButton) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ToggleOffActivityVisibility => UserCommand != null && UserCommand.Type == MTFUserCommandType.ToggleButton ? Visibility.Visible : Visibility.Collapsed;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Command SelectedUserCommandMoveUpCommand => selectedUserCommandMoveUpCommand;
        public Command SelectedUserCommandMoveDownCommand => selectedUserCommandMoveDownCommand;
    }
}
