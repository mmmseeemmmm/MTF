using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceExecution.MainViews
{
    /// <summary>
    /// Interaction logic for ServiceView.xaml
    /// </summary>
    public partial class ServiceView : MTFUserControl
    {
        private bool canResize = true;
        private GridSplitter splitter;


        public ServiceView()
        {
            InitializeComponent();
        }

        private void GridSplitterDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            splitter = sender as GridSplitter;

            canResize = false;
        }

        protected override void GridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            splitter = null;
            base.GridSplitterDragCompleted(sender, e);
        }

        private void ServiceViewGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (canResize)
            {
            }
            else
            {
                if (splitter != null)
                {
                    if (e.PreviousSize.Height > e.NewSize.Height)
                    {
                    }
                    else
                    {
                        splitter.CancelDrag();
                    }
                }
            }


            e.Handled = true;
        }

        
    }
}
