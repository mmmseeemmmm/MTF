using System;
using System.Collections.Generic;

namespace MTFApp.SequenceExecution.ImageHandling
{
    public class TableImagePresenter : ImagePresenter
    {
        private bool enableTableImages;
        private DateTime closingTime;

        public bool EnableTableImages
        {
            get => enableTableImages;
            set
            {
                enableTableImages = value; 
                NotifyPropertyChanged();
            }
        }

        public void ShowTableImages(IEnumerable<ImageContainer> images)
        {
            var now = DateTime.Now;
            if ((now-closingTime).TotalMilliseconds<100)
            {
                return;
            }

            lock (ImagesLock)
            {
                Buffer.Clear();
                Buffer.AddRange(images);
                NotifyPropertyChanged(nameof(ImageCount));
                EnableTableImages = true;
                ImageIndex = 0;
            }
        }


        protected override void Close()
        {
            closingTime = DateTime.Now;
            EnableTableImages = false;
            base.Close();
        }
    }
}
