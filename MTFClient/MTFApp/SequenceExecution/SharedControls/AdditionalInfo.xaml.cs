using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for AdditionalInfo.xaml
    /// </summary>
    public partial class AdditionalInfo : UserControl, INotifyPropertyChanged
    {
        public AdditionalInfo()
        {
            InitializeComponent();
        }

        private int additionalInfoOpenButtonScaleTarget = 1;
        private bool isAdditionalInfoVisible;

        private void ShowIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowAdditionalInfo();
        }

        private void VariablesWatch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdditionaInfoType = AdditionalInfoType.VariablesWatch;
            ShowButton.RaiseEvent(e);
        }

        private void GoldSample_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdditionaInfoType = AdditionalInfoType.GoldSample;
            ShowButton.RaiseEvent(e);
        }


        
        public bool IsAdditionalInfoVisible
        {
            get { return isAdditionalInfoVisible; }
            set
            {
                isAdditionalInfoVisible = value;
                NotifyPropertyChanged();
            }
        }

        private AdditionalInfoType additionaInfoType;
        public AdditionalInfoType AdditionaInfoType
        {
            get { return additionaInfoType; }
            set
            {
                additionaInfoType = value;
                NotifyPropertyChanged();
            }
        }


        private void ShowAdditionalInfo()
        {
                IsAdditionalInfoVisible = !IsAdditionalInfoVisible;
                AdditionalInfoOpenButtonScaleTarget *= -1;
        }

        public int AdditionalInfoOpenButtonScaleTarget
        {
            get { return additionalInfoOpenButtonScaleTarget; }
            set
            {
                additionalInfoOpenButtonScaleTarget = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
