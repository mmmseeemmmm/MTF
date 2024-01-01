using System.Collections.Generic;
using AutomotiveLighting.MTFCommon;

namespace MTFVision.HelperClasses
{
    public class MtfElcomImage:MTFImage
    {
        public MtfElcomImage()
        {
            
        }
        public MtfElcomImage(byte[] imageData)
        {
                RawData = imageData;  
        }
        public byte[] RawData { get; set; }
        public Dictionary<string, string> Config { get; set; }
        public bool IsLarge { get; set; }
    }
}
