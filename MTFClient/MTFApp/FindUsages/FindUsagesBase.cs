using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.FindUsages
{
    public delegate void OnHideEventHandler(object sender, EventArgs e);

    public abstract class FindUsagesBase : Window, INotifyPropertyChanged
    {
        private double scale;
        private readonly double originalWidth;
        private readonly double originalHeight;
        private DisplayModes displayMode = DisplayModes.All;

        protected FindUsagesBase(FindUsagesSetting setting)
        {
            Setting = setting;
            IncludeExternal = true;
            Owner = setting.Owner;
            originalHeight = Height;
            originalWidth = Width;
            if (Owner.DataContext is MainWindowPresenter mainWindowPresenter)
            {
                Scale = mainWindowPresenter.Scale;
            }
            SourceInitialized += InitializeWindowSource;
            MinHeight = 200;
            MinWidth = 330;
            var windowSetting = StoreSettings.GetInstance.SettingsClass.FindUsagesSetting;
            if (!windowSetting.IsEmpty)
            {
                var location = SettingsClass.AdjustLocation(windowSetting.Location, MinWidth, MinHeight);
                Left = location.X;
                Top = location.Y;
                var size = SettingsClass.AdjustSize(windowSetting.Size, location);
                Width = size.Width;
                Height = size.Height;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Height = 300;
                Width = 1000;
            }
        }

        protected readonly FindUsagesSetting Setting;

        public event OnHideEventHandler OnHide;
        public event PropertyChangedEventHandler PropertyChanged;

        #region Override

        protected override void OnClosed(EventArgs e)
        {
            var windowSetting = StoreSettings.GetInstance.SettingsClass.FindUsagesSetting;
            windowSetting.Location = new Point(Left, Top);
            windowSetting.Size = new Size(Width, Height);
            Owner.IsEnabled = true;
            base.OnClosed(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.NumPad0 || e.Key == Key.D0))
            {
                if (Owner.DataContext is MainWindowPresenter mainWindowPresenter)
                {
                    mainWindowPresenter.Scale = 1;
                    Scale = mainWindowPresenter.Scale;
                }

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

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Owner.DataContext is MainWindowPresenter mainWindowPresenter)
                {
                    mainWindowPresenter.Scale = UIHelper.GetNewScale(e.Delta, mainWindowPresenter.Scale);
                    Scale = mainWindowPresenter.Scale;
                }
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Cursor == Cursors.Arrow)
            {
                DragMove();
            }
            base.OnMouseDown(e);
        }
        
        #endregion

        #region Properties

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Height = originalHeight * value;
                Width = originalWidth * value;
                NotifyPropertyChanged();
            }
        }

        public DisplayModes DisplayMode
        {
            get { return displayMode; }
            set
            {
                displayMode = value;
                NotifyPropertyChanged();
            }
        }

        public bool FilterIsSelected
        {
            get { return DisplayMode == DisplayModes.Filtred; }
        }

        public bool AllIsSelected
        {
            get { return DisplayMode == DisplayModes.All; }
        }

        public bool IncludeExternal { get; set; }

        public abstract ICommand RefreshCommand { get; }

        public ICommand SetDisplayModeCommand
        {
            get { return new Command(SetDisplayMode); }
        }

        #endregion



        #region Public methods

        public void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (OnHide != null)
            {
                OnHide(this, EventArgs.Empty);
            }
            Hide();
        }

        public void Select(MTFSequenceActivity activity, MTFSequence sequence)
        {
            Setting.SelectionCallBack(activity, sequence);
        }

        #endregion

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        private void SetDisplayMode(object param)
        {
            if (param != null && Enum.TryParse(param.ToString(), out DisplayModes newDisplayMode))
            {
                DisplayMode = newDisplayMode;
                InvalidateButtons();
            }
        }

        protected virtual void InvalidateButtons()
        {
            NotifyPropertyChanged(nameof(AllIsSelected));
            NotifyPropertyChanged(nameof(FilterIsSelected));
        }

        #region Main window resizing
        private const int resizeBorder = 10;
        private const int resizeCornerBorder = 25;
        private ResizeDirection resizeMode = ResizeDirection.NoResize;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (/*!EnvironmentHelper.IsProductionMode && */WindowState != WindowState.Maximized)
            {
                if (e.LeftButton == MouseButtonState.Pressed && resizeMode != ResizeDirection.NoResize)
                {
                    ResizeWindow(resizeMode);
                }

                resizeMode = GetBorderPosition(e.GetPosition(null));
            }
        }

        private ResizeDirection GetBorderPosition(Point position)
        {
            ResizeDirection mouseBorderPosition;

            //left top corner
            if ((position.X < resizeBorder && position.Y < resizeCornerBorder) || (position.Y < resizeBorder && position.X < resizeCornerBorder))
            {
                Cursor = Cursors.SizeNWSE;
                mouseBorderPosition = ResizeDirection.TopLeft;
            }
            //right top corner
            else if ((position.X > Width - resizeBorder && position.Y < resizeCornerBorder) || (position.X > Width - resizeCornerBorder && position.Y < resizeBorder))
            {
                Cursor = Cursors.SizeNESW;
                mouseBorderPosition = ResizeDirection.TopRight;
            }
            //left bottom corner
            else if (position.X < resizeBorder && position.Y > Height - resizeCornerBorder || position.X < resizeCornerBorder && position.Y > Height - resizeBorder)
            {
                Cursor = Cursors.SizeNESW;
                mouseBorderPosition = ResizeDirection.BottomLeft;
            }
            //right bottom corner
            else if (position.X > Width - resizeCornerBorder && position.Y > Height - resizeBorder || position.X > Width - resizeBorder && position.Y > Height - resizeCornerBorder)
            {
                Cursor = Cursors.SizeNWSE;
                mouseBorderPosition = ResizeDirection.BottomRight;
            }
            //left or right column
            else if (position.X < resizeBorder)
            {
                Cursor = Cursors.SizeWE;
                mouseBorderPosition = ResizeDirection.Left;
            }
            //right column
            else if (position.X > Width - resizeBorder)
            {
                Cursor = Cursors.SizeWE;
                mouseBorderPosition = ResizeDirection.Right;
            }
            //top row
            else if (position.Y < resizeBorder)
            {
                Cursor = Cursors.SizeNS;
                mouseBorderPosition = ResizeDirection.Top;
            }
            //bottom row
            else if (position.Y > Height - resizeBorder)
            {
                Cursor = Cursors.SizeNS;
                mouseBorderPosition = ResizeDirection.Bottom;
            }
            else
            {
                Cursor = Cursors.Arrow;
                mouseBorderPosition = ResizeDirection.NoResize;
            }

            return mouseBorderPosition;
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            NoResize = 100,
        }

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        private void InitializeWindowSource(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion Main window resizing
    }


}
