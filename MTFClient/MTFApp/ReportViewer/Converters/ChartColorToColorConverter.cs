using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.Converters
{
    public class ChartColorToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is ChartColors) ? null : ConvertToBrush((ChartColors)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Brush ConvertToBrush(ChartColors chartColor)
        {
            FrameworkElement element = new FrameworkElement();
            return new SolidColorBrush((Color)element.FindResource(chartColor.ToString()));
        }
            
    }

}
