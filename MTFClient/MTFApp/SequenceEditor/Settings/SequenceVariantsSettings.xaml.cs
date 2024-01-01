using System.Windows;
using System.Windows.Controls;
using MTFApp.SequenceEditor.Settings.SequenceVariant;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for SequenceVariantsSettings.xaml
    /// </summary>
    public partial class SequenceVariantsSettings : UserControl
    {
        //private bool allowApplyVariantChanges = true;
        //private ObservableCollection<SequenceVariantValue> variants;

        //public event PropertyChangedEventHandler PropertyChanged;

        public SequenceVariantsSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        //private void ListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    UIHelper.RaiseScrollEvent(sender, e);
        //}

        public MTFSequence Sequence
        {
            get { return (MTFSequence)GetValue(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(SequenceVariantsSettings),
                new FrameworkPropertyMetadata());

        //private static void SequenceCahnged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = (SequenceVariantsSettings)d;
        //    var sequence = e.NewValue as MTFSequence;
        //    if (sequence != null)
        //    {
        //        control.Variants =
        //            new ObservableCollection<SequenceVariantValue>(
        //                sequence.VariantGroups[0].Values.Select(x => new SequenceVariantValue { Id = x.Id, Name = x.Name }));
        //    }
        //}


        //public ICommand AddVariantCommand => new Command(AddVariant);

        //public ICommand RemoveVariantCommand => new Command(RemoveVariant);

        //public ICommand ApplyVariantChangesCommand => new Command(ApplyVariantChanges);

        //public bool AllowApplyVariantChanges
        //{
        //    get => allowApplyVariantChanges;
        //    set
        //    {
        //        allowApplyVariantChanges = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //private void ApplyVariantChanges()
        //{
        //    if (Sequence != null)
        //    {
        //        AllowApplyVariantChanges = false;
        //        bool canChange = true;

        //        if (ApplyForExternal)
        //        {
        //            var msg = ChangeVariantHandler.CheckVariantsInExternal(Sequence);
        //            if (!string.IsNullOrEmpty(msg))
        //            {
        //                canChange = MTFMessageBox.Show("MTF warning", msg, MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) ==
        //                            MTFMessageBoxResult.Yes;
        //            } 
        //        }

        //        if (canChange)
        //        {
        //            LongTask.Do(() =>
        //                        {
        //                            ChangeVariantHandler.ChangeVariant(Sequence, Sequence.VariantGroups, Variants, ApplyForExternal);
        //                        }, "Changing variants...");
        //        }
        //        AllowApplyVariantChanges = true;
        //    }
        //}


        //private void AddVariant()
        //{
        //    variants.Add(new SequenceVariantValue());
        //}

        //private void RemoveVariant(object param)
        //{
        //    var variant = param as SequenceVariantValue;
        //    if (variant != null)
        //    {
        //        variants.Remove(variant);
        //    }
        //}

        //public ObservableCollection<SequenceVariantValue> Variants
        //{
        //    get { return variants; }
        //    set
        //    {
        //        variants = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //public bool ApplyForExternal { get; set; } = true;


        //protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var control = new SequenceVariantWizard(Sequence);
            var pw = new PopupWindow.PopupWindow(control);
            pw.Show();
        }
    }
}