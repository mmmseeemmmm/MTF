using System.Windows;
using System.Windows.Controls;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for CpuInfo.xaml
    /// </summary>
    public partial class CpuInfo : UserControl
    {
        public CpuInfo()
        {
            InitializeComponent();
            this.root.DataContext = this;
        }

        public MTFClientServerCommon.ComputerInfo.CPUInfo Info
        {
            get { return (MTFClientServerCommon.ComputerInfo.CPUInfo)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(MTFClientServerCommon.ComputerInfo.CPUInfo), typeof(CpuInfo), new PropertyMetadata(null, new PropertyChangedCallback(OnInfoChanged)));

        private static void OnInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CpuInfo)d).Info = (MTFClientServerCommon.ComputerInfo.CPUInfo)e.NewValue;
        }
    }
}
