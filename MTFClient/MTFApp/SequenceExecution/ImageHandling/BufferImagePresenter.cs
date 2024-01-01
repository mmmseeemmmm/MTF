using System;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Helpers;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;

namespace MTFApp.SequenceExecution.ImageHandling
{
    public class BufferImagePresenter : ImagePresenter
    {
        private readonly Action<bool> switchImageDetailMode;
        private bool allowUpdateActual = true;

        public BufferImagePresenter(Action<bool> switchImageDetailMode)
        {
            this.switchImageDetailMode = switchImageDetailMode;
        }

        private const int MaxImages = 200;


        public void Add(ImageContainer imageContainer)
        {
            lock (ImagesLock)
            {
                AddToBuffer(imageContainer);
            }
        }

        public void AddAndSkipExisting(ImageContainer imageContainer)
        {
            lock (ImagesLock)
            {
                if (Buffer.All(x => !GuidHelper.CompareGuidPath(x.GuidPath, imageContainer.GuidPath)))
                {
                    AddToBuffer(imageContainer);
                }
            }
        }

        private void AddToBuffer(ImageContainer imageContainer)
        {
            if (Buffer.Count < MaxImages)
            {
                Buffer.Add(imageContainer);
                NotifyPropertyChanged("ImageCount");
            }
            else
            {
                Buffer.RemoveAt(0);
                Buffer.Add(imageContainer);
            }
            if (allowUpdateActual)
            {
                ImageIndex = Buffer.Count - 1;
            }
        }

        protected override void IndexUpdated()
        {
            allowUpdateActual = ImageCount < 0 || ImageIndex == ImageCount;
            base.IndexUpdated();
        }

        public override void Reset()
        {
            allowUpdateActual = true;
            base.Reset();
        }

        public void SetImageByResult(MTFActivityResult currentResult)
        {
            if (currentResult != null && currentResult.ActivityResult is MTFImage)
            {
                lock (ImagesLock)
                {
                    var img =
                        Buffer.FindLastIndex(
                            x =>
                                x.GuidPath != null &&
                                string.Join(string.Empty, x.GuidPath) == string.Join(string.Empty, currentResult.ActivityIdPath));
                    if (img != -1)
                    {
                        ImageIndex = img;
                    }
                }
            }
        }

        protected override void Close()
        {
            switchImageDetailMode(false);
            base.Close();
        }

        protected override void Open()
        {
            switchImageDetailMode(true);
            base.Open();
        }
    }
}
