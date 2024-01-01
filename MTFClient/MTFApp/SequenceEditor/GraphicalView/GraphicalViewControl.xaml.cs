using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.GraphicalView
{
    /// <summary>
    /// Interaction logic for ImageViewControl.xaml
    /// </summary>
    public partial class GraphicalViewControl : UserControl
    {
        //readonly MTFClient mtfClient = MTFClient.GetMTFClient();

        public GraphicalViewControl()
        {
            InitializeComponent();
            ImageViewRoot.DataContext = this;
        }


        public MTFSequence Sequence
        {
            get { return (MTFSequence)GetValue(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(GraphicalViewControl),
                new PropertyMetadata(null));
    }
}