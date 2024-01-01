using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.PopupWindow;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for SetupVariantControl.xaml
    /// </summary>
    public partial class SetupVariantControl : UserControl, IReturnsDialogResult, IRaiseCloseEvent, INotifyPropertyChanged
    {
        public SetupVariantControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ICommand OkCommand => new Command(OkPressed);

        public ICommand CancelCommand => new Command(CancelPressed);

        public ICommand CleanCommand => new Command(CleanPressed);

        private void OkPressed()
        {
            if (Close != null)
            {
                DialogResult = new MTFDialogResult {Result = MTFDialogResultEnum.Ok};
                Close(this);
            }
        }

        private void CancelPressed()
        {
            if (Close != null)
            {
                DialogResult = new MTFDialogResult { Result = MTFDialogResultEnum.Cancel };
                Close(this);
            }            
        }

        private void CleanPressed()
        {
            foreach (var dataVariant in DataVariants)
            {
                dataVariant.SelectedUsedVariant = null;
                dataVariant.SaveVariant = null;
            }
        }

        public MTFSequence Sequence { get; set; }

        public string ActivityName { get; set; }

        private IEnumerable<DataVariantWrapper> dataVariants;

        public IEnumerable<DataVariantWrapper> DataVariants
        {
            get => dataVariants;
            set
            {
                dataVariants = value;
                Height = 80 + dataVariants.Count()*100;
            }
        }

        public MTFDialogResult DialogResult { get; private set; }
        public event CloseEventHandler Close;
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DataVariantWrapper : NotifyPropertyBase
    {
        private IEnumerable<SequenceVariant> usedDataVariants;
        private SequenceVariant selectedUsedVariant;
        private SequenceVariant saveVariant;
        public string DataName { get; set; }
        public string DisplayDataName { get; set; }

        public IEnumerable<SequenceVariant> UsedDataVariants
        {
            get => usedDataVariants;
            set
            {
                usedDataVariants = value;
                NotifyPropertyChanged();
            }
        }

        public SequenceVariant SelectedUsedVariant
        {
            get => selectedUsedVariant;
            set
            {
                selectedUsedVariant = value;
                if (selectedUsedVariant != null)
                {
                    SaveVariant = selectedUsedVariant.Copy() as SequenceVariant;
                }
                NotifyPropertyChanged();
            }
        }

        public SequenceVariant SaveVariant
        {
            get => saveVariant;
            set
            {
                saveVariant = value;
                NotifyPropertyChanged();
            }
        }


    }
}
