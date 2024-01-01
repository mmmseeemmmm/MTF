using System;
using System.Windows;
using System.Windows.Input;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class MTFImageEditor : MTFEditorBase
    {
        Point? targetStartPoint = null;
        private TouchHelper touch = TouchHelper.Instance;

        public MTFImageEditor()
        {
            InitializeComponent();

            root.DataContext = this;
            
        }


        private void target_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (targetStartPoint != null && Setting.AllowDragDrop)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = (Point)targetStartPoint - mousePos;
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var dragElement = sender as FrameworkElement;
                    DataObject dragData = new DataObject(DragAndDropTypes.GetActivityResult, new object());
                    var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                    if (result != DragDropEffects.None)
                    {
                        var targetActivity = dragData.GetData(DragAndDropTypes.GetActivityResult);
                        var selectedActivity = dragData.GetData(DragAndDropTypes.SelectedActivity);
                        if (targetActivity != null && targetActivity != selectedActivity)
                        {
                            Value = new MTFClientServerCommon.Mathematics.ActivityResultTerm() { Value = targetActivity as MTFClientServerCommon.MTFSequenceActivity };
                        }
                    }
                }
            }
        }

        private void target_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (!(Value is ActivityResultTerm))
                {
                    Value = new ActivityResultTerm(); 
                }
                touch.SetItem(DragAndDropTypes.SetActivityResult, Value, sender);
            }
            else
            {
                targetStartPoint = e.GetPosition(null);
            }
        }

        private void target_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            targetStartPoint = null;
        }

    }
}
