using System;
using System.Collections.Generic;
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

namespace MTFApp.ResultDetail
{
    /// <summary>
    /// Interaction logic for ActivityResultDetail.xaml
    /// </summary>
    public partial class ActivityResultDetail : UserControl
    {
        public ActivityResultDetail()
        {
            InitializeComponent();
        }

        public bool IsActivitySelected
        {
            get { return (bool)GetValue(IsActivitySelectedProperty); }
            set { SetValue(IsActivitySelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActivitySelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActivitySelectedProperty =
            DependencyProperty.Register("IsActivitySelected", typeof(bool), typeof(ActivityResultDetail));

        
    }
}
