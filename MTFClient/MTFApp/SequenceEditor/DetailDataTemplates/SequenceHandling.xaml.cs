using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MTFApp.UIControls;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.DetailDataTemplates
{
    /// <summary>
    /// Interaction logic for SequenceHandling.xaml
    /// </summary>
    public partial class SequenceHandling : UserControl
    {
        public SequenceHandling()
        {
            InitializeComponent();
        }

        private void CheckBox_CheckChange(object sender, RoutedEventArgs e)
        {
            var sequenceHandlingActivity = this.DataContext as MTFSequenceHandlingActivity;
            var checkBox = sender as CheckBox;
            if (sequenceHandlingActivity != null && checkBox != null && sequenceHandlingActivity.CommandsSetting != null)
            {
                foreach (var commandsSetting in sequenceHandlingActivity.CommandsSetting)
                {
                    if (checkBox.IsChecked.HasValue)
                    {
                        commandsSetting.IsEnabled = checkBox.IsChecked.Value;
                    }
                }
            }

        }

        private void ThreeStateCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var sequenceHandlingActivity = this.DataContext as MTFSequenceHandlingActivity;
            var threeStateCheckBox = sender as ThreeStateCheckBox;
            if (sequenceHandlingActivity != null && threeStateCheckBox != null && sequenceHandlingActivity.CommandsSetting != null)
            {
                foreach (var commandsSetting in sequenceHandlingActivity.CommandsSetting)
                {
                    commandsSetting.IsChecked = threeStateCheckBox.IsChecked;
                }
            }
        }

        private void IsAllowed_Click(object sender, RoutedEventArgs e)
        {
            var sequenceHandlingActivity = this.DataContext as MTFSequenceHandlingActivity;
            var threeStateCheckBox = sender as ThreeStateCheckBox;
            if (sequenceHandlingActivity != null && threeStateCheckBox != null )
            {
                sequenceHandlingActivity.UserCommandsSetting?.Where(s => s.UserCommand.Type == MTFUserCommandType.Button || s.UserCommand.Type == MTFUserCommandType.ToggleButton)
                    .ToList().ForEach(s => s.IsEnabled = threeStateCheckBox.IsChecked);
            }
        }

        private void IsChecked_Click(object sender, RoutedEventArgs e)
        {
            var sequenceHandlingActivity = this.DataContext as MTFSequenceHandlingActivity;
            var threeStateCheckBox = sender as ThreeStateCheckBox;
            if (sequenceHandlingActivity != null && threeStateCheckBox != null)
            {
                sequenceHandlingActivity.UserCommandsSetting?.Where(s => s.UserCommand.Type == MTFUserCommandType.ToggleButton)
                    .ToList().ForEach(s => s.IsChecked = threeStateCheckBox.IsChecked);
            }
        }
    }
}
