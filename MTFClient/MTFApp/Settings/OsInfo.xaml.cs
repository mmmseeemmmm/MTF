using System.Windows;
using System.Windows.Controls;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for OsInfo.xaml
    /// </summary>
    public partial class OsInfo : UserControl
    {
        public OsInfo()
        {
            InitializeComponent();
            this.root.DataContext = this;
        }
        
        public MTFClientServerCommon.ComputerInfo.OSInfo Info
        {
            get { return (MTFClientServerCommon.ComputerInfo.OSInfo)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(MTFClientServerCommon.ComputerInfo.OSInfo), typeof(OsInfo), new PropertyMetadata(null, new PropertyChangedCallback(OnInfoChanged)));

        private static void OnInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OsInfo)d).Info = (MTFClientServerCommon.ComputerInfo.OSInfo)e.NewValue;
        }
    }
}
