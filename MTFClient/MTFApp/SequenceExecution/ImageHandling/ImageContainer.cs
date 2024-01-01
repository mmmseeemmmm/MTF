using System;

namespace MTFApp.SequenceExecution.ImageHandling
{
    public class ImageContainer
    {
        private byte[] imageData;

        public byte[] ImageData
        {
            get { return imageData; }
            private set { imageData = value; }
        }
        private Guid[] guidPath;

        public Guid[] GuidPath
        {
            get { return guidPath; }
            private set { guidPath = value; }
        }


        public ImageContainer(byte[] imageData, Guid[] guidPath)
        {
            this.imageData = imageData;
            this.guidPath = guidPath;
        }
    }
}
