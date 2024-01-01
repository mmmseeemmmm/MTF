namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    public class TextPanel : PanelBase
    {
        public string Text { get; set; }
        public int FontSize { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public override object Clone()
        {
            return new TextPanel
            {
                Text = Text,
                FontSize =  FontSize,
                TextAlignment = TextAlignment,
            };
        }
    }
}
