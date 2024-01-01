using MTFClientServerCommon;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace MTFApp.MTFWizard
{
    public abstract class MTFWizardUserControl : UserControl, INotifyPropertyChanged
    {
        protected readonly MTFClient MTFClient;
        private bool allowFinnis = true;

        public MTFWizardUserControl()
        {
            this.MTFClient = MTFClient.GetMTFClient();
        }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string WizardType { get; }

        public virtual bool Skip
        {
            get { return false; }
        }

        public virtual bool CanNext => !IsLastControl;

        public virtual bool IsLastControl => false;

        public virtual bool IsFirstControl => false;

        public bool CanFinnish => AllowFinnish && IsLastControl;

        public virtual bool AllowFinnish
        {
            get => allowFinnis;
            set
            {
                allowFinnis = value;
                NotifyPropertyChanged("CanFinnish");
            }
        }

        public void Show()
        {
            this.OnShow();
        }

        public void Hide()
        {
            this.OnHide();
        }

        public bool Finish()
        {
            return this.OnFinis();
        }

        protected virtual bool OnFinis()
        {
            return true;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        public virtual Button UserButton1 => null;

        public virtual Button UserButton2 => null;

        public virtual Button UserButton3 => null;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}