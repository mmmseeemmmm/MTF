using System.Windows;
using System.Windows.Controls;

namespace MTFApp.Settings
{
    /// <summary>
    /// Interaction logic for ApplicationInfo.xaml
    /// </summary>
    public partial class ApplicationInfo : UserControl
    {
        public ApplicationInfo()
        {
            InitializeComponent();
            this.root.DataContext = this;
        }

        public MTFClientServerCommon.ComputerInfo.ApplicationInfo Info
        {
            get { return (MTFClientServerCommon.ComputerInfo.ApplicationInfo)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(MTFClientServerCommon.ComputerInfo.ApplicationInfo), typeof(ApplicationInfo), new PropertyMetadata(null, new PropertyChangedCallback(OnInfoChanged)));

        private static void OnInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ApplicationInfo)d).Info = (MTFClientServerCommon.ComputerInfo.ApplicationInfo)e.NewValue;
        }
    }
}
