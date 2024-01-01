using System;
using System.Globalization;
using System.Windows.Data;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.Converters
{
    class TextAlignmentToWpfAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is TextAlignment) ? System.Windows.TextAlignment.Left : ConvertToWpfAlignment((TextAlignment) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private System.Windows.TextAlignment ConvertToWpfAlignment(TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Left: return System.Windows.TextAlignment.Left;
                case TextAlignment.Center: return System.Windows.TextAlignment.Center;
                case TextAlignment.Right: return System.Windows.TextAlignment.Right;
                default: return System.Windows.TextAlignment.Left;
            }
        }
    }
}
