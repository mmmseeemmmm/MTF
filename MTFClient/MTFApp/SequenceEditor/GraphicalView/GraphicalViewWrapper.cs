using System;
using MTFApp.UIHelpers;
using MTFClientServerCommon.GraphicalView;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class GraphicalViewWrapper : NotifyPropertyBase
    {
        private bool isNew = true;
        private GraphicalViewImg img;
        private GraphicalViewInfo graphicalViewInfo = new GraphicalViewInfo();
        public event ImageChangedEventHandler ImageChanged;

        public bool IsNew
        {
            get => isNew;
            set
            {
                isNew = value;
                NotifyPropertyChanged();
            }
        }

        public GraphicalViewImg Img
        {
            get => img;
            set
            {
                if (img != null)
                {
                    ImageChanged?.Invoke(this, EventArgs.Empty);
                }

                img = value;
                NotifyPropertyChanged();

                if (value != null)
                {
                    graphicalViewInfo.ImageFileName = img.FileName;
                }
            }
        }

        public GraphicalViewInfo GraphicalViewInfo
        {
            get => graphicalViewInfo;
            set => graphicalViewInfo = value;
        }
    }

    public delegate void ImageChangedEventHandler(object sender, EventArgs e);
}