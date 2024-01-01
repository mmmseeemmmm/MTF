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
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports.PanelViews
{
    /// <summary>
    /// Interaction logic for TextPanelView.xaml
    /// </summary>
    public partial class TextPanelView : MTFUserControl
    {
        public TextPanelView()
        {
            InitializeComponent();
            textPanelRoot.DataContext = this;
        }

        public static readonly DependencyProperty TextPanelProperty = DependencyProperty.Register(
            "TextPanel", typeof(TextPanel), typeof(TextPanelView), new PropertyMetadata(default(TextPanel)));

        public TextPanel TextPanel
        {
            get => (TextPanel) GetValue(TextPanelProperty);
            set => SetValue(TextPanelProperty, value);
        }
    }
}
