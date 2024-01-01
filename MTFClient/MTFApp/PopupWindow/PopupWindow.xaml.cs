using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon.UIHelpers;

namespace MTFApp.PopupWindow
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window, INotifyPropertyChanged
    {
        private readonly double originalWidth;
        private readonly double originalHeight;
        private readonly UserControl innerControl;
        private readonly Action<MTFDialogResult, List<Guid>> sendResult;
        private readonly List<Guid> activityPath;
        private readonly MainWindowPresenter mainWindowPresenter = null;
        private bool isBlockingDialog = true;
        private readonly Action closeAction;
        private double dialogScaleSetting = 1;

        public PopupWindow(UserControl innerControl)
            : this(innerControl, true)
        {
        }

        public PopupWindow(Action<MTFDialogResult, List<Guid>> sendResult, List<Guid> activityPath, UserControl innerControl, bool isBlockingDialog)
            : this(innerControl, true, null, isBlockingDialog)
        {
            this.sendResult = sendResult;
            this.activityPath = activityPath;
        }

        public PopupWindow(UserControl innerControl, bool showHeader)
            : this(innerControl, showHeader, null)
        {
        }

        public PopupWindow(UserControl innerControl, bool showHeader, Window owner)
            : this(innerControl, showHeader, owner, true)
        {
        }

        public PopupWindow(UserControl innerControl, bool showHeader, Window owner, bool isBlockingDialog):
            this(innerControl, showHeader, owner, isBlockingDialog, null)
        {
            
        }

        public PopupWindow(UserControl innerControl, bool showHeader, Window owner, bool isBlockingDialog, Action closeAction)
        {
            this.showHeader = showHeader;
            DataContext = this;
            this.innerControl = innerControl;
            this.isBlockingDialog = isBlockingDialog;
            this.closeAction = closeAction;

            var setting = StoreSettings.GetInstance.SettingsClass;
            if (setting != null)
            {
                dialogScaleSetting = setting.DialogScale;
            }

            InitializeComponent();
            try
            {
                Owner = owner == null ? Application.Current.MainWindow : owner;
            }
            catch { }

            if (this.showHeader)
            {
                Height = innerControl.Height + 30; //+ header height
            }
            else
            {
                Height = innerControl.Height;
            }
            Width = innerControl.Width;
            content.Content = innerControl;

            if (innerControl is IRaiseCloseEvent)
            {
                ((IRaiseCloseEvent)innerControl).Close += PopupWindow_Close;
            }
            originalHeight = Height;
            originalWidth = Width;
            if (Owner != null && (Owner.DataContext as MainWindowPresenter) != null)
            {
                mainWindowPresenter = Owner.DataContext as MainWindowPresenter;
                Scale = (Owner.DataContext as MainWindowPresenter).Scale;
                if (isBlockingDialog)
                {
                    mainWindowPresenter.IsDarken = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (closeAction!=null)
            {
                closeAction.Invoke();
            }
            base.OnClosed(e);
        }


        public MessageBoxControl.MessageBoxControl GetContent()
        {
            return content.Content as MessageBoxControl.MessageBoxControl;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                (Owner.DataContext as MainWindowPresenter).Scale = UIHelpers.UIHelper.GetNewScale(e.Delta, (Owner.DataContext as MainWindowPresenter).Scale);
                Scale = (Owner.DataContext as MainWindowPresenter).Scale;
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.NumPad0 || e.Key == Key.D0))
            {
                (Owner.DataContext as MainWindowPresenter).Scale = 1;
                Scale = (Owner.DataContext as MainWindowPresenter).Scale;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        public MTFDialogResult MTFDialogResult
        {
            get;
            set;
        }


        private double scale;

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value * dialogScaleSetting;
                Height = originalHeight * scale;
                Width = originalWidth * scale;
                NotifyPropertyChanged();
            }
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (innerControl is IOnClosing onClosingInnerControl)
            {
                if (!onClosingInnerControl.OnClosing())
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (!canClose)
            {
                e.Cancel = true;
                return;
            }
            if (mainWindowPresenter != null && isBlockingDialog)
            {
                mainWindowPresenter.IsDarken = false;
            }
            base.OnClosing(e);
        }

        private bool canClose = true;
        public bool CanClose
        {
            get { return canClose; }
            set { canClose = value; NotifyPropertyChanged(); }
        }

        private bool showHeader = true;
        public bool ShowHeader
        {
            get { return showHeader; }
        }

        void PopupWindow_Close(object sender)
        {
            if (innerControl is IReturnsDialogResult result)
            {
                this.MTFDialogResult = result.DialogResult;
            }

            sendResult?.Invoke(MTFDialogResult, activityPath);
            canClose = true;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MTFDialogResult = null;
            sendResult?.Invoke(MTFDialogResult, activityPath);
            canClose = true;
            this.Close();
        }
    }
}
