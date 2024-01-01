using MTFApp.UIHelpers;
using System.Windows;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : MTFUserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            IsVisibleChanged += SettingsControl_IsVisibleChanged;
        }

        void SettingsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var presenter = DataContext as SettingsPresenter;
            if (presenter != null)
            {
                presenter.DoCyclicUpdate = IsVisible;
            }
        }
    }
}
