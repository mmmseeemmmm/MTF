using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for DeviceUnderTestSettings.xaml
    /// </summary>
    public partial class DeviceUnderTestSettings : UserControl
    {
        public DeviceUnderTestSettings()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public MTFSequence Sequence
        {
            get { return (MTFSequence)GetValue(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(DeviceUnderTestSettings),
                new FrameworkPropertyMetadata { PropertyChangedCallback = SequenceCahnged });

        private static void SequenceCahnged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MTFSequence sequence)
            {
                if (sequence.DeviceUnderTestInfos == null)
                {
                    sequence.DeviceUnderTestInfos = new MTFObservableCollection<DeviceUnderTestInfo>();
                }
            }
        }

        public ICommand AddDutCommand => new Command(AddDut);
        public ICommand RemoveDutCommand => new Command(RemoveDut);

        private void AddDut()
        {
            Sequence.DeviceUnderTestInfos.Add(new DeviceUnderTestInfo());
        }

        private void RemoveDut(object param)
        {
            if (param is DeviceUnderTestInfo dut)
            {
                Sequence.DeviceUnderTestInfos.Remove(dut);
            }
        }
    }
}
