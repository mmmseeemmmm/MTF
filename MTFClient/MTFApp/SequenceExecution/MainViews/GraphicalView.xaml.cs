using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.SequenceExecution.GraphicalViewHandling;

namespace MTFApp.SequenceExecution.MainViews
{
    /// <summary>
    /// Interaction logic for GraphicalView.xaml
    /// </summary>
    public partial class GraphicalView : UserControl, INotifyPropertyChanged
    {
        private double scale = 1;
        private readonly double pointSize = 30;
        public event PropertyChangedEventHandler PropertyChanged;

        public GraphicalView()
        {
            InitializeComponent();
            GraphicalViewRoot.DataContext = this;
        }


        public double PointSize => pointSize;

        public double Scale
        {
            get => scale;
            set
            {
                scale = double.IsNaN(value) || double.IsInfinity(value) ? 1 : value;
                NotifyPropertyChanged();
            }
        }


        public ICommand OpenTableCommand
        {
            get => (ICommand)GetValue(OpenTableCommandProperty);
            set => SetValue(OpenTableCommandProperty, value);
        }

        public static readonly DependencyProperty OpenTableCommandProperty =
            DependencyProperty.Register("OpenTableCommand", typeof(ICommand), typeof(GraphicalView),
                new PropertyMetadata(null));


        public ExecutionGraphicalViewWrapper CurrentGraphicalView
        {
            get => (ExecutionGraphicalViewWrapper)GetValue(CurrentGraphicalViewProperty);
            set => SetValue(CurrentGraphicalViewProperty, value);
        }

        public static readonly DependencyProperty CurrentGraphicalViewProperty =
            DependencyProperty.Register("CurrentGraphicalView", typeof(ExecutionGraphicalViewWrapper), typeof(GraphicalView),
                new PropertyMetadata(null));

        private void ViewBoxOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewBox = (Viewbox)sender;
            if (viewBox.Child is FrameworkElement child)
            {
                SetScale(child.ActualWidth / viewBox.ActualWidth);
            }
        }

        private void ViewBoxChildOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var child = (FrameworkElement)sender;
            if (child.Parent is Viewbox parent)
            {
                SetScale(child.ActualWidth / parent.ActualWidth);
            }
        }

        private void SetScale(double value)
        {
            if (value != scale)
            {
                Scale = value;
            }
        }


        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TestItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            if (fe.DataContext is ExecutionGraphicalViewTestedItem item)
            {
                OpenTableCommand?.Execute(item);
            }
        }


        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            if (fe.DataContext is ExecutionGraphicalViewTestedItem testItem && testItem.ValidationTable != null)
            {
                if (testItem.ValidationTable.IsCollapsed)
                {
                    testItem.ValidationTable.IsCollapsed = false;
                }
            }
        }
    }
}