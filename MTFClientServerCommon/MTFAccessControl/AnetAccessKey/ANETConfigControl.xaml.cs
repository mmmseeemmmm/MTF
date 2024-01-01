using MTFClientServerCommon.UIHelpers;
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

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    /// <summary>
    /// Interaction logic for ANETConfigControl.xaml
    /// </summary>
    public partial class ANETConfigControl : UserControl, IOnClosing
    {
        public ANETConfigControl(AnetAccessKeyProvider provider)
        {
            DataContext = new ANETConfigPresenter(provider);
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (grid.SelectedItem != null)
            {
                grid.ScrollIntoView(grid.SelectedItem);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Count() > 1)
            {
                ((ANETConfigPresenter)DataContext).RenameRole(((TextBox)sender).Text);
            }

        }

        public bool OnClosing()
        {
            ((ANETConfigPresenter)DataContext).OnClose();
            return true;
        }
    }
}
