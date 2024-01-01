using System.Collections.Generic;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceExecution.ImageHandling
{
    public class ImagePresenter : NotifyPropertyBase
    {
        protected readonly object ImagesLock = new object();
        private readonly Command goNextCommad;
        private readonly Command goPreviousCommad;
        protected readonly List<ImageContainer> Buffer = new List<ImageContainer>();
        private int imageIndex;

        public ImagePresenter()
        {
            goNextCommad = new Command(GoNext, () => CanNext);
            goPreviousCommad = new Command(GoPrevious, () => CanPrevious);
        }

        public int ImageCount
        {
            get
            {
                NotifyPropertyChanged(nameof(SliderIsEnabled));
                return Buffer.Count - 1;
            }
        }

        public bool SliderIsEnabled => Buffer.Count > 0;

        public byte[] CurrentImage
        {
            get
            {
                if (Buffer.Count > 0 && ImageIndex >= 0 && ImageIndex < Buffer.Count)
                {
                    lock (ImagesLock)
                    {
                        return Buffer[ImageIndex].ImageData;
                    }
                }
                return null;
            }
        }

        public bool CanNext => ImageIndex < Buffer.Count - 1;

        public bool CanPrevious => ImageIndex > 0;


        public int ImageIndex
        {
            get => imageIndex;
            set
            {
                imageIndex = value;
                IndexUpdated();
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentImage));
                UIHelper.InvokeOnDispatcherAsync(() => goNextCommad.RaiseCanExecuteChanged());
                UIHelper.InvokeOnDispatcherAsync(() => goPreviousCommad.RaiseCanExecuteChanged());
            }
        }


        protected virtual void IndexUpdated()
        {

        }

        public virtual void Reset()
        {
            lock (ImagesLock)
            {
                Buffer.Clear();
            }
            ImageIndex = 0;
            NotifyPropertyChanged(nameof(ImageCount));
            NotifyPropertyChanged(nameof(CurrentImage));
        }

        public ICommand CloseCommand => new Command(Close);

        protected virtual void Close()
        {
        }

        public ICommand OpenCommand => new Command(Open);

        protected virtual void Open()
        {
        }

        public ICommand NextCommand => goNextCommad;

        private void GoNext()
        {
            ChangeImage(true);
        }

        public ICommand PreviousCommand => goPreviousCommad;

        private void GoPrevious()
        {
            ChangeImage(false);
        }


        public void ChangeImage(bool next)
        {
            if (next)
            {
                var nextIndex = ImageIndex + 1;
                if (nextIndex < Buffer.Count)
                {
                    ImageIndex = nextIndex;
                }
            }
            else
            {
                var previousIndex = ImageIndex - 1;
                if (previousIndex >= 0)
                {
                    ImageIndex = previousIndex;
                }
            }
        }
    }
}
