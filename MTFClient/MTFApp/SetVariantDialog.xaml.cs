using System.Windows;
using System.Windows.Controls;
using MTFApp.PopupWindow;
using MTFClientServerCommon;

namespace MTFApp
{
    /// <summary>
    /// Interaction logic for SetVariantDialog.xaml
    /// </summary>
    public partial class SetVariantDialog : UserControl, IReturnsDialogResult, IRaiseCloseEvent
    {
        private MTFSequence sequence;

        public SetVariantDialog(MTFSequence sequence, SequenceVariant variant)
        {
            InitializeComponent();
            Root.DataContext = this;
            this.sequence = sequence;
            SequenceVariant = variant;
        }


        public MTFSequence Sequence
        {
            get { return sequence; }
        }

        

        public SequenceVariant SequenceVariant
        {
            get { return (SequenceVariant)GetValue(SequenceVariantProperty); }
            set { SetValue(SequenceVariantProperty, value); }
        }

        public static readonly DependencyProperty SequenceVariantProperty =
            DependencyProperty.Register("SequenceVariant", typeof(SequenceVariant), typeof(SetVariantDialog), new FrameworkPropertyMetadata());


        private void CloseWindow()
        {
            if (Close != null)
            {
                Close(this);
            }
        }


        public MTFDialogResult DialogResult { get; private set; }

        public event CloseEventHandler Close;

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = new MTFDialogResult {Result = MTFDialogResultEnum.Cancel};
            CloseWindow();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = new MTFDialogResult { Result = MTFDialogResultEnum.Ok };
            CloseWindow();
        }

        private void BtnRemoveClick(object sender, RoutedEventArgs e)
        {
            SequenceVariant = null;
        }
    }
}
