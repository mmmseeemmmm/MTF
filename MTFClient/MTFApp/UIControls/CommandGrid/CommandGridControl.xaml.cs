using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon;
using Point = System.Drawing.Point;

namespace MTFApp.UIControls.CommandGrid
{
    /// <summary>
    /// Interaction logic for CommandGridControl.xaml
    /// </summary>
    public partial class CommandGridControl : UserControl, INotifyPropertyChanged
    {
        private List<List<MTFServiceCommandWrapper>> commandGrid;
        private bool columsAreReady;
        private bool rowsAreReady;
        private bool commandsAreReady;
        private bool gridIsGenerated;
        private bool gridIsFilled;
        private bool isMarked;
        private bool serviceModeVariantIsReady;
        private bool commandGridModeIsReady;
        private MTFServiceModeVariants serviceModeVariant;
        private CommandGridMode mode;
        private const int ButtonMargin = 21;
        private const int LeftScrollbarMargin = 20;

        public CommandGridControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ActualWidthProperty && Mode == CommandGridMode.Execution && gridIsFilled)
            {
                int columnsCount = ColumnsCount;
                Task.Run(() => ChangeWidth(ActualWidth - LeftScrollbarMargin, columnsCount));
            }
            base.OnPropertyChanged(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        #region DependencyProperty

        public IList<MTFServiceCommandWrapper> ExecutionCommands
        {
            get { return (IList<MTFServiceCommandWrapper>)GetValue(ExecutionCommandsProperty); }
            set { SetValue(ExecutionCommandsProperty, value); }
        }

        public static readonly DependencyProperty ExecutionCommandsProperty =
            DependencyProperty.Register("ExecutionCommands", typeof(IList<MTFServiceCommandWrapper>), typeof(CommandGridControl),
            new FrameworkPropertyMetadata { PropertyChangedCallback = ExecutionCommandChanged });

        private static void ExecutionCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CommandGridControl;
            if (control != null && e.NewValue != null)
            {
                control.commandsAreReady = true;
                control.GenerateGrid();
                control.FillGrid();
            }
        }


        public MTFObservableCollection<MTFServiceCommand> AllCommands
        {
            get { return (MTFObservableCollection<MTFServiceCommand>)GetValue(AllCommandsProperty); }
            set { SetValue(AllCommandsProperty, value); }
        }

        public static readonly DependencyProperty AllCommandsProperty =
            DependencyProperty.Register("AllCommands", typeof(MTFObservableCollection<MTFServiceCommand>), typeof(CommandGridControl),
            new FrameworkPropertyMetadata { PropertyChangedCallback = AllCommandsChanged });

        private static void AllCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CommandGridControl;
            if (control != null)
            {
                control.commandsAreReady = true;
                control.GenerateGrid();
                control.FillGrid();
            }
        }

        public MTFServiceCommand ServiceCommand
        {
            get { return (MTFServiceCommand)GetValue(ServiceCommandProperty); }
            set { SetValue(ServiceCommandProperty, value); }
        }

        public static readonly DependencyProperty ServiceCommandProperty =
            DependencyProperty.Register("ServiceCommand", typeof(MTFServiceCommand), typeof(CommandGridControl),
            new FrameworkPropertyMetadata { PropertyChangedCallback = ServiceCommandChanged });

        private static void ServiceCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CommandGridControl;
            if (control != null && control.ServiceCommand != null)
            {
                control.isMarked = false;
                control.GenerateGrid();
                control.FillGrid();
                control.MarkActual();
            }
        }

        public int ColumnsCount
        {
            get { return (int)GetValue(ColumnsCountProperty); }
            set { SetValue(ColumnsCountProperty, value); }
        }

        public static readonly DependencyProperty ColumnsCountProperty =
            DependencyProperty.Register("ColumnsCount", typeof(int), typeof(CommandGridControl),
            new FrameworkPropertyMetadata { PropertyChangedCallback = ColumnsChanged });

        private static void ColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CommandGridControl;
            if (control != null && (control.ServiceCommand != null || control.Mode == CommandGridMode.Execution))
            {
                control.gridIsGenerated = false;
                control.gridIsFilled = false;
                control.columsAreReady = true;
                control.GenerateGrid();
                control.FillGrid();
            }
        }

        public int RowsCount
        {
            get { return (int)GetValue(RowsCountProperty); }
            set { SetValue(RowsCountProperty, value); }
        }

        public static readonly DependencyProperty RowsCountProperty =
            DependencyProperty.Register("RowsCount", typeof(int), typeof(CommandGridControl),
            new FrameworkPropertyMetadata { PropertyChangedCallback = RowsChanged });

        private static void RowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CommandGridControl;
            if (control != null && (control.ServiceCommand != null || control.Mode == CommandGridMode.Execution))
            {
                control.gridIsGenerated = false;
                control.gridIsFilled = false;
                control.rowsAreReady = true;
                control.GenerateGrid();
                control.FillGrid();
            }
        }

        public bool ShowLabels
        {
            get { return (bool)GetValue(ShowLabelsProperty); }
            set { SetValue(ShowLabelsProperty, value); }
        }

        public static readonly DependencyProperty ShowLabelsProperty =
            DependencyProperty.Register("ShowLabels", typeof(bool), typeof(CommandGridControl), new PropertyMetadata(true));

        

        #endregion

        #region Properties

        private bool CanGenerateGrid
        {
            get { return columsAreReady && rowsAreReady; }
        }

        public MTFServiceModeVariants ServiceModeVariant
        {
            get { return serviceModeVariant; }
            set
            {
                serviceModeVariant = value;
                serviceModeVariantIsReady = true;
                GenerateGrid();
                FillGrid();
            }
        }

        public CommandGridMode Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                commandGridModeIsReady = true;
                GenerateGrid();
                FillGrid();
            }
        }

        public List<List<MTFServiceCommandWrapper>> CommandGrid
        {
            get { return commandGrid; }
        }


        #endregion

        #region Private methods

        private void ChangeWidth(double currentWidth, int columnsCount)
        {
            var width = (int)currentWidth / columnsCount - ButtonMargin;
            if (commandGrid != null)
            {
                foreach (var row in commandGrid)
                {
                    foreach (var commandWrapper in row)
                    {
                        commandWrapper.Width = width;
                    }
                }
            }
        }

        private void FillGrid()
        {
            if (gridIsGenerated && commandsAreReady && !gridIsFilled && serviceModeVariantIsReady && commandGridModeIsReady)
            {
                gridIsFilled = true;
                switch (Mode)
                {
                    case CommandGridMode.Execution:
                        FillExecutionCommands();
                        break;
                    case CommandGridMode.Editor:
                        FillAllCommands();
                        break;
                    //case CommandGridMode.ReadOnly:
                    //    break;
                    //default:
                    //    throw new ArgumentOutOfRangeException();
                }
                MarkActual();
            }
        }

        private void FillExecutionCommands()
        {
            if (ExecutionCommands != null)
            {
                var width = ((int)ActualWidth - LeftScrollbarMargin) / ColumnsCount - ButtonMargin;
                foreach (var executionCommand in ExecutionCommands)
                {
                    var location = ServiceModeVariant == MTFServiceModeVariants.ServiceMode ? executionCommand.Command.ServiceLocation : executionCommand.Command.TeachLocation;
                    if (!location.IsEmpty)
                    {
                        int x = location.X - 1;
                        int y = location.Y - 1;
                        if (x >= 0 && x < commandGrid.Count && y >= 0 && y < commandGrid[x].Count)
                        {
                            commandGrid[x][y] = executionCommand;
                        }
                    }
                }
            }
        }

        private void FillAllCommands()
        {
            foreach (var command in AllCommands)
            {
                var location = ServiceModeVariant == MTFServiceModeVariants.ServiceMode ? command.ServiceLocation : command.TeachLocation;
                if (!location.IsEmpty)
                {
                    int x = location.X - 1;
                    int y = location.Y - 1;
                    if (x >= 0 && x < commandGrid.Count && y >= 0 && y < commandGrid[x].Count)
                    {
                        commandGrid[x][y].Command = command;
                    }
                    else
                    {
                        switch (ServiceModeVariant)
                        {
                            case MTFServiceModeVariants.ServiceMode:
                                command.ServiceLocation = Point.Empty;
                                break;
                            case MTFServiceModeVariants.Teach:
                                command.TeachLocation = Point.Empty;
                                break;
                        }
                    }
                }
            }
        }


        private void MarkActual()
        {
            if (commandGrid != null && ServiceCommand != null && !isMarked)
            {
                isMarked = true;
                foreach (var row in commandGrid)
                {
                    foreach (var commandWrapper in row)
                    {
                        if (commandWrapper != null)
                        {
                            commandWrapper.IsActual = commandWrapper.Command == ServiceCommand;
                        }
                    }
                }
            }
        }

        private void GenerateGrid()
        {
            if (CanGenerateGrid && !gridIsGenerated)
            {
                gridIsGenerated = true;
                isMarked = false;
                gridIsFilled = false;
                commandGrid = new List<List<MTFServiceCommandWrapper>>(RowsCount);
                for (int i = 0; i < RowsCount; i++)
                {
                    var columns = new List<MTFServiceCommandWrapper>(ColumnsCount);
                    for (int j = 0; j < ColumnsCount; j++)
                    {
                        columns.Add(new MTFServiceCommandWrapper(null, i + 1, j + 1));
                    }
                    commandGrid.Add(columns);
                }
                NotifyPropertyChanged("CommandGrid");
            }
        }


        private void Square_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as FrameworkElement;
            if (border != null && ServiceCommand != null)
            {
                var item = border.DataContext as MTFServiceCommandWrapper;
                if (item != null)
                {
                    if (item.Command == null)
                    {
                        var location = ServiceModeVariant == MTFServiceModeVariants.ServiceMode ? ServiceCommand.ServiceLocation : ServiceCommand.TeachLocation;
                        if (!location.IsEmpty)
                        {
                            UnMarktCommand(location);
                        }
                        item.Command = ServiceCommand;
                        item.IsActual = true;
                        switch (ServiceModeVariant)
                        {
                            case MTFServiceModeVariants.ServiceMode:
                                ServiceCommand.ServiceLocation = new Point(item.X, item.Y);
                                break;
                            case MTFServiceModeVariants.Teach:
                                ServiceCommand.TeachLocation = new Point(item.X, item.Y);
                                break;
                        }
                    }
                    else
                    {
                        if (item.Command == ServiceCommand)
                        {
                            item.Command = null;
                        }
                    }
                }
            }
        }

        private void UnMarktCommand(Point location)
        {
            if (commandGrid != null)
            {
                if (location.X > 0 && location.X <= commandGrid.Count)
                {
                    var row = commandGrid[location.X - 1];
                    if (location.Y > 0 && location.Y <= row.Count)
                    {
                        var commandWrapper = row[location.Y - 1];
                        if (commandWrapper != null)
                        {
                            commandWrapper.Command = null;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
