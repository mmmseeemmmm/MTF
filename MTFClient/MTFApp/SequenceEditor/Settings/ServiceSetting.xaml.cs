using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for ServiceSetting.xaml
    /// </summary>
    public partial class ServiceSetting : UserControl
    {
        public ServiceSetting()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }



        public MTFServiceDesignSetting ServiceDesignSetting
        {
            get { return (MTFServiceDesignSetting)GetValue(ServiceDesignSettingProperty); }
            set { SetValue(ServiceDesignSettingProperty, value); }
        }

        public static readonly DependencyProperty ServiceDesignSettingProperty =
            DependencyProperty.Register("ServiceDesignSetting", typeof(MTFServiceDesignSetting), typeof(ServiceSetting), new FrameworkPropertyMetadata());

        

        

        
    }
}
