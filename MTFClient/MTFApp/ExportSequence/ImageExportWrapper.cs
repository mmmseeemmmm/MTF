using MTFClientServerCommon.GraphicalView;

namespace MTFApp.ExportSequence
{
    public class ImageExportWrapper
    {
        public GraphicalViewImg Img { get; set; }
        public ExportSetting ExportSetting { get; set; }

        public bool IsAssigned
        {
            get { return Img != null; }
        }
    }
}