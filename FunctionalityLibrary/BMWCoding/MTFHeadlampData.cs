using System.Collections.Generic;
using System.Linq;
using AL_Dat_Client;
using AutomotiveLighting.MTFCommon;

namespace BMWCoding
{
    [MTFKnownClass]
    public class MTFHeadlampData
    {
        public static MTFHeadlampData GetHeadlampData(HeadlampData headlampData)
        {
            return headlampData == null
                ? null
                : new MTFHeadlampData
                  {
                      Project = MTFProject.GetProject(headlampData.project),
                      VariantName = headlampData.variant_name,
                      FA = headlampData.fa,
                      AlPartNr = headlampData.al_pn,
                      AlChangeIndex = headlampData.al_ai,
                      CustomerPn = headlampData.customer_pn,
                      CustomerAi = headlampData.customer_ai,
                      MountingSide = headlampData.mounting_side,
                      EdiabasVariant = headlampData.ediabas_variant,
                      ECUs = headlampData.ECUs == null ? null : headlampData.ECUs.Select(MTFECU.GetEcu).ToList(),
                  };
        }

        public MTFProject Project { get; set; }
        public string VariantName { get; set; }
        public string FA { get; set; }
        public string AlPartNr { get; set; }
        public string AlChangeIndex { get; set; }
        public string CustomerPn { get; set; }
        public string CustomerAi { get; set; }
        public string MountingSide { get; set; }
        public List<MTFECU> ECUs { get; set; }
        public string EdiabasVariant { get; set; }
    }
    
}
