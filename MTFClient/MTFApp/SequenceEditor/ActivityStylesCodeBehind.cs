using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor
{
    public partial class ActivityStylesCodeBehind : ResourceDictionary
    {
        private Point? ActivityResultTargetStartPoint = null;
        private DragAndDrop dragAndDrop = DragAndDrop.Instance;
        private TouchHelper touch;


        public void AcitivityNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var item = UIHelper.FindParent<ListBoxItem>(textBox);
                    if (item != null)
                    {
                        item.Focus();
                    }
                }
            }
        }

        private void ActivityResultTarget_MouseMove(object sender, MouseEventArgs e)
        {
            SettingsClass setting = StoreSettings.GetInstance.SettingsClass;
            if (ActivityResultTargetStartPoint == null || !setting.AllowDragDrop)
            {
                return;
            }
            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)ActivityResultTargetStartPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var dragElement = sender as FrameworkElement;
                //object data = Value;
                var data = dragElement.Tag;
                // Initialize the drag & drop operation
                DataObject dragData = new DataObject(DragAndDropTypes.GetActivityToCall, data);
                var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                if (result != DragDropEffects.None)
                {
                    var activity = dragData.GetData(DragAndDropTypes.GetActivityToCall) as MTFSequenceActivity;
                    if (activity != null && activity != data)
                    {
                        (data as MTFExecuteActivity).ActivityToCall = activity;
                    }
                }
            }
        }

        private void ActivityResultTarget_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (touch == null)
                {
                    touch = TouchHelper.Instance;
                }
                var data = (sender as FrameworkElement).Tag;
                if (data is MTFExecuteActivity)
                {
                    touch.SetItem(DragAndDropTypes.SetActivity, data, sender);
                }
            }
            else
            {
                dragAndDrop.DisableDragAndDrop();
                ActivityResultTargetStartPoint = e.GetPosition(null);
            }
        }

        private void ActivityResultTarget_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ActivityResultTargetStartPoint = null;
        }
    }
}
