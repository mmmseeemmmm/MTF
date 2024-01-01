using System.Windows.Controls;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for RoundingRulesSettings.xaml
    /// </summary>
    public partial class RoundingRulesSettings : UserControl
    {
        public RoundingRulesSettings()
        {
            InitializeComponent();
        }

        private void ListBox_OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            UIHelpers.UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
