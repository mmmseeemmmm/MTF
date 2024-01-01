using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.MainViews
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class TreeView : MTFUserControl
    {
        private Point? dragDropStartPoint;
        public TreeView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            try
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
            catch (Exception)
            {
                //                Information for developers (use Text Visualizer to read this):
                //This exception was thrown because the generator for control 'System.Windows.Controls.ListBox Items.Count:10' with name '(unnamed)' has received sequence of CollectionChanged events that do not agree with the current state of the Items collection.  The following differences were detected:
                //  Accumulated count 9 is different from actual count 10.  [Accumulated count is (Count at last Reset + #Adds - #Removes since last Reset).]
                //  At index 7:  Generator's item 'MTFClientServerCommon.MTFActivityVisualisationWrapper' is different from actual item 'MTFClientServerCommon.MTFActivityVisualisationWrapper'.
                //  At index 8:  Generator's item 'MTFClientServerCommon.MTFActivityVisualisationWrapper' is different from actual item 'MTFClientServerCommon.MTFActivityVisualisationWrapper'.

                //One or more of the following sources may have raised the wrong events:
                //     System.Windows.Controls.ItemContainerGenerator
                //      System.Windows.Controls.ItemCollection
            }
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null)
            {
                return;
            }

            t.ScrollToEnd();
        }


        private void ExecutingPosition_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragDropStartPoint = e.GetPosition(null);
        }

        private void ExecutingPosition_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragDropStartPoint == null)
            {
                return;
            }
            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)dragDropStartPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var element = sender as FrameworkElement;
                if (element != null)
                {
                    var sourceData = new DataObject("ExecutionPosition", element.Tag);
                    DragDrop.DoDragDrop(element, sourceData, DragDropEffects.All);
                    dragDropStartPoint = null;
                }
            }

        }


        private void ExecutingPosition_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragDropStartPoint = null;
        }

        private void NewExecutionPosition_Drop(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var target = element.Tag as MTFActivityVisualisationWrapper;
            if (target != null)
            {
                if (e.Data.GetDataPresent("ExecutionPosition"))
                {
                    var originalItem = e.Data.GetData("ExecutionPosition") as MTFActivityVisualisationWrapper;
                    if (originalItem != null && originalItem != target)
                    {
                        originalItem.IsExecutionPointer = false;
                        target.IsExecutionPointer = true;
                        var dataContext = DataContext as SequenceExecutionPresenter;
                        if (dataContext != null)
                        {
                            dataContext.SetExecutionPosition(target);
                        }
                    }
                }
            }
        }

        private void ActivityStatusOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }

            var fe = (FrameworkElement)sender;
            var activityWrapper = fe.DataContext as MTFActivityVisualisationWrapper;
            if (activityWrapper != null && activityWrapper.IsExpandable && activityWrapper.Status == MTFExecutionActivityStatus.Nok)
            {
                var dataContext = DataContext as SequenceExecutionPresenter;
                if (dataContext != null)
                {
                    dataContext.TreeViewManager.ExpandToSourceErrorItem(activityWrapper);
                }
            }
        }

    }
}
