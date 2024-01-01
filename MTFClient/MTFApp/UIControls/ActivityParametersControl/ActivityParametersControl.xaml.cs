using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIControls.ActivityParametersControl
{
    /// <summary>
    /// Interaction logic for ActivityParametersControl.xaml
    /// </summary>
    public partial class ActivityParametersControl : UserControl
    {
        public ActivityParametersControl()
        {
            InitializeComponent();
            ActivityParametersControlRoot.DataContext = this;
        }

        private void ScrollParentListBox(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }

        public bool UseFullTerms { get; set; }


        public IList<MTFParameterValue> ActivityParameters
        {
            get => (IList<MTFParameterValue>)GetValue(ActivityParametersProperty);
            set => SetValue(ActivityParametersProperty, value);
        }

        public static readonly DependencyProperty ActivityParametersProperty =
            DependencyProperty.Register("ActivityParameters", typeof(IList<MTFParameterValue>), typeof(ActivityParametersControl),
                new PropertyMetadata(null));


        public Term SelectedTerm
        {
            get => (Term)GetValue(SelectedTermProperty);
            set => SetValue(SelectedTermProperty, value);
        }

        public static readonly DependencyProperty SelectedTermProperty =
            DependencyProperty.Register("SelectedTerm", typeof(Term), typeof(ActivityParametersControl), new PropertyMetadata(null));


        public bool UsedInTermDesigner
        {
            get => (bool)GetValue(UsedInTermDesignerProperty);
            set => SetValue(UsedInTermDesignerProperty, value);
        }

        public static readonly DependencyProperty UsedInTermDesignerProperty =
            DependencyProperty.Register("UsedInTermDesigner", typeof(bool), typeof(ActivityParametersControl), new PropertyMetadata(false));
    }
}