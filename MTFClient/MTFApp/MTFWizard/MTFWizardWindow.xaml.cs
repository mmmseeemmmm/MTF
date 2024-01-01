using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MTFApp.MTFWizard
{
    /// <summary>
    /// Interaction logic for MTFWizardWindow.xaml
    /// </summary>
    public partial class MTFWizardWindow : Window
    {
        readonly MainWindowPresenter mainWindowPresenter = null;

        public MTFWizardWindow(List<MTFWizardUserControl> controls)
        {
            InitializeComponent();
            this.DataContext = new MTFWizardPresenter(controls);
            if (App.Current.MainWindow != null && App.Current.MainWindow.DataContext is MainWindowPresenter)
            {
                mainWindowPresenter = (MainWindowPresenter)App.Current.MainWindow.DataContext;
                mainWindowPresenter.IsDarken = true;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Result = (this.DataContext as MTFWizardPresenter).DialogResult;
            if (mainWindowPresenter != null)
            {
                mainWindowPresenter.IsDarken = false;
            }
        }

        public bool? Result { get; private set; }

        private void Content_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelpers.UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
