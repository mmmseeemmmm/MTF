using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MTFApp.UIHelpers;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;
using MTFClientServerCommon.GraphicalView;

namespace MTFApp.SequenceEditor.GraphicalView
{
    /// <summary>
    /// Interaction logic for GraphicalViewEditor.xaml
    /// </summary>
    public partial class GraphicalViewEditor : UserControl, ITouchHelper
    {
        private readonly GraphicalViewEditorPresenter presenter;
        private readonly SettingsClass setting;
        private readonly UserControl trashControl = new UserControl();
        private bool? isOverTrash;
        private TouchHelper touch;
        private Point? dragStartPoint;

        public GraphicalViewEditor()
        {
            InitializeComponent();
            presenter = new GraphicalViewEditorPresenter();
            DataContext = presenter;
            setting = StoreSettings.GetInstance.SettingsClass;
            trashControl.Style = this.FindResource("IconTrash") as Style;
        }


        public MTFSequence Sequence
        {
            get => (MTFSequence)GetValue(SequenceProperty);
            set => SetValue(SequenceProperty, value);
        }

        public UserControl TrashControl => trashControl;

        private TouchHelper Touch => touch ?? (touch = TouchHelper.Instance);

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(GraphicalViewEditor),
                new FrameworkPropertyMetadata(OnSequenceChanged));

        private static void OnSequenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (GraphicalViewEditor)d;
            source.SequenceChanged(e.NewValue as MTFSequence);
        }

        private void SequenceChanged(MTFSequence sequence)
        {
            presenter.SequenceChanged(sequence);
        }

        private void TableListBoxPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!setting.AllowDragDrop || dragStartPoint == null)
            {
                return;
            }

            Point mousePos = e.GetPosition(sender as IInputElement);
            Vector diff = (Point)dragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var item = UIHelper.FindParent<ContentPresenter>(e.MouseDevice.DirectlyOver as DependencyObject);
                if (item != null && item.Content is GraphicalViewTableItemBase tableWrapper)
                {
                    var data = new DataObject(DragAndDropTypes.ValidationTable, tableWrapper);
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                    dragStartPoint = null;
                }
            }
        }

        private void PictureDrop(object sender, DragEventArgs e)
        {
            if (sender is Canvas canvas && e.Data.GetDataPresent(DragAndDropTypes.ValidationTable))
            {
                var table = e.Data.GetData(DragAndDropTypes.ValidationTable);
                var position = e.GetPosition(canvas);
                presenter.AddPoint(position, table as GraphicalViewTableItemBase, GraphicalViewTestItemType.Table);
            }
        }


        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.Source is Thumb thumb)
            {
                var mousePosition = Mouse.GetPosition(TrashControl);
                var currentlyIsOver = TrashControl.InputHitTest(mousePosition) != null;

                if (!isOverTrash.HasValue || currentlyIsOver != isOverTrash.Value)
                {
                    ChangeTrashStyle(currentlyIsOver);
                    isOverTrash = currentlyIsOver;
                }

                presenter.Move(thumb.DataContext as GraphicalViewTestItem, e.HorizontalChange, e.VerticalChange);
            }
        }

        private void ChangeTrashStyle(bool currentlyIsOver)
        {
            TrashControl.Foreground = this.FindResource(currentlyIsOver ? "ALRedBrush" : "ALBlackBrush") as Brush;
        }

        private void TabControlOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = UIHelper.FindParent<TabItem>(e.MouseDevice.DirectlyOver as Visual);
            if (item?.Content is GraphicalViewWrapper data && data.IsNew)
            {
                presenter.CreateNewView(data);
                e.Handled = true;
            }
        }

        private void PictureWrapperGridPreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DragAndDropTypes.ResourceManagerImage))
            {
                presenter.SetImage(e.Data.GetData(DragAndDropTypes.ResourceManagerImage) as GraphicalViewImg);
                e.Handled = true;
            }
        }

        private void ViewBoxChildOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var child = (FrameworkElement)sender;
            if (child.Parent is Viewbox parent)
            {
                presenter.SetScale(child.ActualWidth / parent.ActualWidth);
                presenter.RecalculateItems(e.PreviousSize, e.NewSize);
            }
        }

        private void ThumbOnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(TrashControl);
            var currentlyIsOver = TrashControl.InputHitTest(mousePosition) != null;
            var thumb = (Thumb)sender;
            var item = thumb.DataContext as GraphicalViewTestItem;

            if (currentlyIsOver)
            {
                presenter.RemoveTestItem(item);
                ChangeTrashStyle(false);
            }
            else
            {
                var canvas = UIHelper.FindParent<Canvas>(thumb);
                if (canvas != null)
                {
                    presenter.ValidatePosition(item, canvas.ActualWidth, canvas.ActualHeight);
                }
            }
        }

        private void ViewBoxOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewBox = (Viewbox)sender;
            if (viewBox.Child is FrameworkElement child)
            {
                presenter.SetScale(child.ActualWidth / viewBox.ActualWidth);
            }
        }

        private void TableOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = UIHelper.FindParent<Button>(e.MouseDevice.DirectlyOver as Visual);
            if (item == null)
            {
                if (e.ClickCount == 2)
                {
                    var fe = (FrameworkElement)sender;

                    Touch.SetItem(DragAndDropTypes.GraphicalViewTable, fe.DataContext, sender);
                }
                else
                {
                    dragStartPoint = e.GetPosition(sender as IInputElement);
                }
            }
        }

        private void PictureOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Touch.DataObject != null)
            {
                if (sender is Canvas canvas)
                {
                    var position = e.MouseDevice.GetPosition(canvas);
                    if (Touch.DataObject.GetData(DragAndDropTypes.GraphicalViewTable) is GraphicalViewTableItemBase itemBase)
                    {
                        presenter.AddPoint(position, itemBase, GraphicalViewTestItemType.Table);
                    }
                    else
                    {
                        if (Touch.DataObject.GetData(DragAndDropTypes.GraphicalViewTestItem) is GraphicalViewTestItem testItem)
                        {
                            presenter.Place(testItem, position);
                            presenter.ValidatePosition(testItem, canvas.ActualWidth, canvas.ActualHeight);
                        }
                    }

                    touch.Clear();
                }
            }
        }

        private void TestItemOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var fe = (FrameworkElement)sender;

                Touch.SetItem(DragAndDropTypes.GraphicalViewTestItem, fe.DataContext, sender, this);
            }
        }


        public object SourceElement { get; set; }

        public void Select()
        {
            if (SourceElement is Ellipse ellipse)
            {
                ellipse.Fill = FindResource("ALRedBrush") as Brush;
            }
        }

        public void Unselect()
        {
            if (SourceElement is Ellipse ellipse)
            {
                ellipse.Fill = FindResource("ALYellowBrush") as Brush;
            }
        }

        private void TableOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = null;
        }
    }
}