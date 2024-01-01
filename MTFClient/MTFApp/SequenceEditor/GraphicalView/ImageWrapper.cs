using MTFApp.UIHelpers;
using MTFClientServerCommon.GraphicalView;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class ImageWrapper : NotifyPropertyBase
    {
        private GraphicalViewImg img;
        private bool isUsed;

        public ImageWrapper(GraphicalViewImg img)
        {
            this.img = img;
        }

        public GraphicalViewImg Img
        {
            get { return img; }
            set { img = value; }
        }

        public bool IsUsed
        {
            get { return isUsed; }
            set
            {
                isUsed = value;
                NotifyPropertyChanged();
            }
        }
    }
}