using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceEditor.DetailDataTemplates
{
    /// <summary>
    /// Interaction logic for MTFSequenceClassInfo.xaml
    /// </summary>
    public partial class MTFSequenceClassInfo : UserControl
    {
        public MTFSequenceClassInfo()
        {
            InitializeComponent();
        }

        private void CollapsingButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cp = sender as ContentPresenter;
            if (cp != null)
            {
                var dataPackage = cp.Content as DataPackageWrapper;
                if (dataPackage != null)
                {
                    dataPackage.IsCollapsed = !dataPackage.IsCollapsed;
                }
            }
        }

        private void ScrollParentListBox(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
