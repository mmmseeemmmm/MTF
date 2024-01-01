using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFActivityTargetEditor.xaml
    /// </summary>
    public partial class MTFActivityTargetEditor : MTFEditorBase
    {
        private MTFSequenceActivity activity;

        public MTFActivityTargetEditor()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public MTFSequenceActivity Activity
        {
            get
            {
                //return activity;
                return GetActivity();
            }
            set
            {
                SetActivity(value);
                activity = value;
                NotifyPropertyChanged();
            }
        }

        private void SetActivity(MTFSequenceActivity value)
        {
            var term = Value as ActivityTargetTerm;
            if (term != null)
            {
                term.Value = value;
            }
            else
            {
                Value = new ActivityTargetTerm { Value = value };
            }
        }

        private MTFSequenceActivity GetActivity()
        {
            var term = Value as ActivityTargetTerm;
            return term != null ? term.Value : null;
        }

        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(source, e);
        }
    }
}
