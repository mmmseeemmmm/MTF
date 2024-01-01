using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MTFApp.ComponentConfig
{
    /// <summary>
    /// Interaction logic for ParameterTabControl.xaml
    /// </summary>
    public partial class ParameterTabControl : UserControl
    {
        public ParameterTabControl()
        {
            InitializeComponent();

            tabControl.SelectedIndex = 0;
        }

        int lastSelectedTab = 0;
        int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value != -1)
                {
                    lastSelectedTab = value;
                }
                selectedIndex = value;
            }
        }

        private void tabControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            Dispatcher.BeginInvoke(new Action(() => { tabControl.SelectedIndex = lastSelectedTab; }), null);
        }
    }
}
