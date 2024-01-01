using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTFApp.SequenceEditor.DetailDataTemplates
{
    /// <summary>
    /// Interaction logic for DetailHeader.xaml
    /// </summary>
    public partial class DetailHeader : UserControl
    {
        public DetailHeader()
        {
            InitializeComponent();
            Root.DataContext = this;
            this.Background = FindResource("ALYellowBrush") as Brush;
            Height = (double)FindResource("ItemHeight");
        }



        public string DetailType
        {
            get { return (string)GetValue(DetailTypeProperty); }
            set { SetValue(DetailTypeProperty, value); }
        }

        public static readonly DependencyProperty DetailTypeProperty =
            DependencyProperty.Register("DetailType", typeof(string), typeof(DetailHeader), new PropertyMetadata(null));

        

    }
}
