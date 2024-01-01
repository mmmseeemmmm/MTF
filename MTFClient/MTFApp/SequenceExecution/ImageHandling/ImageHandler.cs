using System;
using System.Collections.Generic;
using System.Linq;
using MTFApp.UIHelpers;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;

namespace MTFApp.SequenceExecution.ImageHandling
{
    public class ImageHandler : NotifyPropertyBase
    {
        private readonly BufferImagePresenter imageBuffer;
        private readonly TableImagePresenter tableImages = new TableImagePresenter();

        public ImageHandler(Action<bool> switchImageDetailMode)
        {
            imageBuffer = new BufferImagePresenter(switchImageDetailMode);
        }

        public BufferImagePresenter ImageBuffer
        {
            get { return imageBuffer; }
        }

        public TableImagePresenter TableImages
        {
            get { return tableImages; }
        }

        public void AddToBuffer(byte[] imageData, Guid[] guidPath)
        {
            imageBuffer.Add(new ImageContainer(imageData, guidPath));
        }

        public void AddLoadedResultToBuffer(byte[] imageData, Guid[] guidPath)
        {
            imageBuffer.AddAndSkipExisting(new ImageContainer(imageData, guidPath));
        }

        public void ShowTableImages(List<byte[]> images)
        {
            if (images != null)
            {
                tableImages.ShowTableImages(images.Select(x => new ImageContainer(x, null)).ToArray());
            }
        }

        public void Reset()
        {
            imageBuffer.Reset();
            tableImages.Reset();
        }

        public void SetImagByResult(MTFActivityResult currentResult)
        {
            imageBuffer.SetImageByResult(currentResult);
        }
    }
    
}
