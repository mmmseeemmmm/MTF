using System.Collections.Generic;
using AutomotiveLighting.MTFCommon;
using AutomotiveLighting.DaimlerZenZefi;
using MTFZenZefi.Structures;

namespace MTFZenZefi
{
    [MTFClass(Name = "ZenZefi", Icon = MTFIcons.Certificate)]
    [MTFClassCategory("Communication")]
    public class ZenZefiComponent
    {
        private readonly ZenZefi zenZefi;
        [MTFConstructor]
        public ZenZefiComponent(bool runAsService)
        {
            this.zenZefi = new ZenZefi(runAsService ? RunMode.Service : RunMode.Cmd);
        }

        [MTFMethod(DisplayName = "Get Variant Coding Certificate")]
        public byte[] GetVariantCodingCertificate(string ecuName, bool getLatest)
        {
            return this.zenZefi.GetVariantCodingCertificate(ecuName, getLatest);
        }

        [MTFMethod(DisplayName = "Get Variant Coding Signature")]
        public CodingSignature GetVariantCodingSignature(string ecuName, string vin, byte[] data, bool useLatest)
        {
            var result = zenZefi.GetVariantCodingSignature(ecuName, vin, data, useLatest);

            return FillVariantCodingSignature(result);
        }

        private CodingSignature FillVariantCodingSignature(Dictionary<string, byte[]> result)
        {
            var output = new CodingSignature();

            if (result != null)
            {
                if (result.ContainsKey("signature"))
                {
                    output.Signature = result["signature"];
                }

                if (result.ContainsKey("serialNumber"))
                {
                    output.SerialNumber = result["serialNumber"];
                }
            }

            return output;
        }
    }
}
