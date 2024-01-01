using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for AccessKeyProviderSettings.xaml
    /// </summary>
    public partial class AccessKeyProviderControl : UserControl
    {
        public AccessKeyProviderControl()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
