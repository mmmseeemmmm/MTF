using MTFVision.Common.VisionBasicObjects;
using Newtonsoft.Json;

namespace MTFVision.DoMlfDetection
{
    class SelectedDetectionArea
    {
        public ushort areaType { get; set; }
        public Point center { get; set; }
        [JsonProperty(PropertyName = "+horizontalSize")]
        public float plusHorizontalSize { get; set; }
        [JsonProperty(PropertyName = "-horizontalSize")]
        public float minusHorizontalSize { get; set; }
        [JsonProperty(PropertyName = "+verticalSize")]
        public float plusVerticalSize { get; set; }
        [JsonProperty(PropertyName = "-verticalSize")]
        public float minusVerticalSize { get; set; }
    }
}
