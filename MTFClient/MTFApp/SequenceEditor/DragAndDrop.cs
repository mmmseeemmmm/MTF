using System.Windows;

namespace MTFApp.SequenceEditor
{
    public class DragAndDrop
    {
        private static readonly DragAndDrop instance = new DragAndDrop();
        private Point? startPoint;


        private DragAndDrop()
        {

        }

        public static DragAndDrop Instance
        {
            get { return instance; }
        }

        public bool IsEnableDragAndDrop
        {
            get { return startPoint != null; }
        }

        public void DisableDragAndDrop()
        {
            startPoint = null;
        }

        public void EnableDragAndDrop(Point point)
        {
            startPoint = point;
        }

        public Point StartPoint
        {
            get
            {
                if (startPoint.HasValue)
                {
                    return startPoint.Value;
                }
                return new Point(0, 0);
            }
        }


    }
}
