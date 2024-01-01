using System.Windows.Controls;
using System.Windows.Input;

namespace MTFApp.ResultDetail
{
    /// <summary>
    /// Interaction logic for ActivityHeaderParameters.xaml
    /// </summary>
    public partial class ActivityHeaderParameters : UserControl
    {
        public ActivityHeaderParameters()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelpers.UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
