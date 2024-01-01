using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace MTFApp.SequenceExecution.ActivityProgress
{
    class ActivityProgressImage : ActivityProgressBar
    {
        private int imageCounter = 0;
        private Stopwatch sw = new Stopwatch();
        private byte[] imageData;
        public byte[] ImageData
        {
            get { return imageData; }
            set
            {
                imageData = value;
                if (imageData != null)
                {
                    if (!sw.IsRunning)
                    {
                        sw.Start();
                    }
                    imageCounter++;

                    NotifyPropertyChanged("Image");
                    NotifyPropertyChanged("Fps");
                }
            }
        }

        private BitmapImage image;
        public BitmapImage Image
        {
            get
            {
                if (imageData != null)
                {
                    image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(imageData);
                    image.EndInit();
                }

                return image;
            }
        }

        public float Fps
        {
            get { return (float)imageCounter / ((float)sw.ElapsedMilliseconds / 1000); }
        }
    }
}
