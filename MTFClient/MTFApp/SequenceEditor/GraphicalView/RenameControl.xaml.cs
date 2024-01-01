using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.PopupWindow;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.GraphicalView
{
    /// <summary>
    /// Interaction logic for RenameControl.xaml
    /// </summary>
    public partial class RenameControl : UserControl, IRaiseCloseEvent, IReturnsDialogResult
    {
        public event CloseEventHandler Close;

        public RenameControl()
            : this(null, null)
        {
        }

        public RenameControl(object obj, string propertyToChange)
        {
            InitializeComponent();
            RenameControlRoot.DataContext = this;

            if (obj != null)
            {
                ParentObject = obj;
            }
            if (propertyToChange != null)
            {
                PropertyToChange = propertyToChange;
            }
        }

        public string PropertyToChange
        {
            get { return (string)GetValue(PropertyToChangeProperty); }
            set { SetValue(PropertyToChangeProperty, value); }
        }

        public static readonly DependencyProperty PropertyToChangeProperty =
            DependencyProperty.Register("PropertyToChange", typeof(string), typeof(RenameControl), new PropertyMetadata(null));


        public object ParentObject
        {
            get { return GetValue(ParentObjectProperty); }
            set { SetValue(ParentObjectProperty, value); }
        }

        public static readonly DependencyProperty ParentObjectProperty =
            DependencyProperty.Register("ParentObject", typeof(object), typeof(RenameControl), new PropertyMetadata(null));


        public ICommand OkCommand
        {
            get { return new Command(Ok); }
        }

        public ICommand CancelCommand
        {
            get { return new Command(Cancel); }
        }

        public string Value { get; set; }

        public MTFDialogResult DialogResult { get; private set; }


        private void Ok()
        {
            DialogResult = new MTFDialogResult {Result = MTFDialogResultEnum.Ok};
            if (ParentObject != null && PropertyToChange != null)
            {
                var type = ParentObject.GetType();
                var property = type.GetProperty(PropertyToChange);
                if (property != null && property.PropertyType == typeof(string))
                {
                    property.SetValue(ParentObject, Value);
                }
            }
            CloseDialog();
        }

        private void Cancel()
        {
            DialogResult = new MTFDialogResult {Result = MTFDialogResultEnum.Cancel};
            CloseDialog();
        }

        private void CloseDialog()
        {
            if (Close != null)
            {
                Close(this);
            }
        }
    }
}