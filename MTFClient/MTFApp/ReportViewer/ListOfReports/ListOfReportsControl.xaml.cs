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
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.ReportViewer.ListOfReports
{
    /// <summary>
    /// Interaction logic for ListOfReportsControl.xaml
    /// </summary>
    public partial class ListOfReportsControl : MTFUserControl
    {
        public ListOfReportsControl()
        {
            InitializeComponent();

            DataContext = new ListOfReportsPresenter();
        }

        private void ReportMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ((ListOfReportsPresenter)DataContext).OpenDetail(((FrameworkElement)sender).DataContext as SequenceReportPreview);
            }
        }
    }
}
