using AL_Dat_Client;
using AutomotiveLighting.MTFCommon;

namespace BMWCoding
{
    [MTFKnownClass]
    public class MTFECU
    {
        public static MTFECU GetEcu(ECU ecu)
        {
            return ecu == null
                ? null
                : new MTFECU
                  {
                      CdFileName = ecu.cd_fileName,
                      Name = ecu.name,
                      DiagAddress = ecu.diag_address,
                      Customer = ecu.customer,
                      Istep = ecu.i_step,
                      HWEL = ecu.hwel,
                      SWFL = ecu.swfl,
                      CAFD = ecu.cafd,
                      BTLD = ecu.btld,
                      CAFDShip = ecu.cafd_ship,
                      PdxContainer = ecu.pdx_container,
                      PdxBaseVariant = ecu.pdx_base_variant,
                      SGDB = ecu.sgdb,
                      AlPartNr = ecu.al_pn,
                      AlChangeIndex = ecu.al_ai,
                      CustomerPn = ecu.customer_pn,
                      CustomerAi = ecu.customer_ai,
                  };
        }
        public string CdFileName { get; set; }
        public string Name { get; set; }
        public string DiagAddress { get; set; }
        public string Customer { get; set; }
        public string Istep { get; set; }
        public string HWEL { get; set; }
        public string SWFL { get; set; }
        public string CAFD { get; set; }
        public string BTLD { get; set; }
        public string CAFDShip { get; set; }
        public string PdxContainer { get; set; }
        public string PdxBaseVariant { get; set; }
        public string SGDB { get; set; }
        public string AlPartNr { get; set; }
        public string AlChangeIndex { get; set; }
        public string CustomerPn { get; set; }
        public string CustomerAi { get; set; }
    }
}