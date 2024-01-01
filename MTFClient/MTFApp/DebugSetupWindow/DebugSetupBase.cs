using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MTFApp.SequenceExecution.Helpers;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.DebugSetupWindow
{
    public abstract class DebugSetupBase : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<ExtendedModeActivitySetting> dataCollection;
        private readonly Action<MTFActivityVisualisationWrapper> setSelectedItem;

        private int height = 200;
        private int width = 1200;
        private ExtendedModeActivitySetting selectedItem;
        private string searchText;
        private bool onlyActivePoints;
        
        private Command closeCommand;
        private Command clearSearchTextCommand;
        private Command enableAllBreakPointsCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DebugSetupBase(ObservableCollection<ExtendedModeActivitySetting> dataCollection, Action<MTFActivityVisualisationWrapper> setSelectedItem)
        {
            this.dataCollection = dataCollection;
            this.setSelectedItem = setSelectedItem;
            SourceInitialized += InitializeWindowSource;

            MinHeight = 80;
            MinWidth = 400;
            OnlyActivePoints = false;
        }

        protected virtual void InitCommands()
        {
            closeCommand = new Command(Close);
            clearSearchTextCommand = new Command(() => SearchText = null);
            enableAllBreakPointsCommand = new Command(EnableAllBreakPoints);
        }

        protected abstract void EnableAllBreakPoints();

        protected WindowSetting WindowSetting { get; set; }

        public IEnumerable<ExtendedModeActivitySetting> FiltredItems => GetFiltredItems();

        protected virtual IEnumerable<ExtendedModeActivitySetting> GetFiltredItems()
        {
            IEnumerable<ExtendedModeActivitySetting> output = DataCollection;

            if (OnlyActivePoints)
            {
                output = output.Where(x => x.State == StateDebugSetup.Active);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                output = output.Where(x => x.ActivityName.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            return output;
        }

        public bool OnlyActivePoints
        {
            get => onlyActivePoints;
            set
            {
                onlyActivePoints = value;
                NotifyPropertyChanged();
                NotifyFiltredItems();
            }
        }

        public ObservableCollection<ExtendedModeActivitySetting> DataCollection => dataCollection;

        public ICommand CloseCommand => closeCommand;

        public Command ClearSearchTextCommand => clearSearchTextCommand;

        public Command EnableAllPointsCommand => enableAllBreakPointsCommand;

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                NotifyFiltredItems();
            }
        }

        public virtual bool ShowFiltredItems => OnlyActivePoints || !string.IsNullOrEmpty(SearchText);

        public ExtendedModeActivitySetting SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                if (value != null && setSelectedItem != null && value.ActivityWrapper != null)
                {
                    setSelectedItem(value.ActivityWrapper);
                }
                SelectionChanged();
                NotifyPropertyChanged();
            }
        }

        protected virtual void SelectionChanged()
        {
            
        }


        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        protected void AdjustSize()
        {
            if (!WindowSetting.IsEmpty)
            {
                var location = SettingsClass.AdjustLocation(WindowSetting.Location, width, height);
                Left = location.X;
                Top = location.Y;
                var size = SettingsClass.AdjustSize(WindowSetting.Size, location);
                Width = size.Width;
                Height = size.Height;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Height = height;
                Width = width;
            }
        }

        protected void NotifyFiltredItems()
        {
            NotifyPropertyChanged(nameof(FiltredItems));
            NotifyPropertyChanged(nameof(ShowFiltredItems));
        }

        #region Override

        protected override void OnClosed(EventArgs e)
        {
            WindowSetting.Location = new Point(Left, Top);
            WindowSetting.Size = new Size(Width, Height);
            this.IsEnabled = true;
            base.OnClosed(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.NumPad0 || e.Key == Key.D0))
            {
                var mainWindowPresenter = this.DataContext as MainWindowPresenter;
                if (mainWindowPresenter != null)
                {
                    mainWindowPresenter.Scale = 1;
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
                var mainWindowPresenter = this.DataContext as MainWindowPresenter;
                if (mainWindowPresenter != null)
                {
                    mainWindowPresenter.Scale = UIHelper.GetNewScale(e.Delta, mainWindowPresenter.Scale);
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
