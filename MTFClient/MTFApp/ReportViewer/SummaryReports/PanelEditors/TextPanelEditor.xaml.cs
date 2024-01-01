using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using TextAlignment = MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport.TextAlignment;

namespace MTFApp.ReportViewer.SummaryReports.PanelEditors
{
    /// <summary>
    /// Interaction logic for TextPanelEditor.xaml
    /// </summary>
    public partial class TextPanelEditor : MTFUserControl, INotifyPropertyChanged
    {
        public TextPanelEditor()
        {
            InitializeComponent();
            textPanelEditorRoot.DataContext = this;
        }

        public static readonly DependencyProperty TextPanelProperty = DependencyProperty.Register(
            "TextPanel", typeof(TextPanel), typeof(TextPanelEditor), new PropertyMetadata(default(TextPanel)));

        public TextPanel TextPanel
        {
            get => (TextPanel) GetValue(TextPanelProperty);
            set => SetValue(TextPanelProperty, value);
        }

        public bool TextAlignmentLeft
        {
            get => TextPanel?.TextAlignment == TextAlignment.Left;
            set
            {
                if (!value)
                {
                    return;
                }

                TextPanel.TextAlignment = TextAlignment.Left;
                NotifyTextAlignmentChanged();
            }
        }

        public bool TextAlignmentRight
        {
            get => TextPanel?.TextAlignment == TextAlignment.Right;
            set
            {
                if (!value)
                {
                    return;
                }

                TextPanel.TextAlignment = TextAlignment.Right;
                NotifyTextAlignmentChanged();
            }
        }

        public bool TextAlignmentCenter
        {
            get => TextPanel?.TextAlignment == TextAlignment.Center;
            set
            {
                if (!value)
                {
                    return;
                }

                TextPanel.TextAlignment = TextAlignment.Center;
                NotifyTextAlignmentChanged();
            }
        }

        private void NotifyTextAlignmentChanged()
        {
            OnPropertyChanged(nameof(TextAlignmentLeft));
            OnPropertyChanged(nameof(TextAlignmentRight));
            OnPropertyChanged(nameof(TextAlignmentCenter));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
