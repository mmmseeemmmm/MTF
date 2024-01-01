using System;
using System.Runtime.Serialization;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace AutomotiveLighting.MTFCommon
{
    [Serializable]
    public class MTFImage
    {
        public MTFImage()
        {

        }
        public MTFImage(SerializationInfo info, StreamingContext context) 
        {
        }


        public MTFImage(byte[] imageData)
        {
            this.imageData = imageData;
        }

        public MTFImage(Image image)
        {
            ImageConverter converter = new ImageConverter();
            imageData = (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        public MTFImage(BitmapImage bitmapImage)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            imageData = data;
        }


        private byte[] imageData;

        public byte[] ImageData
        {
            get { return imageData; }
            set { imageData = value; }
        }

        public override string ToString()
        {
            return "Image";
        }
        
    }
}
