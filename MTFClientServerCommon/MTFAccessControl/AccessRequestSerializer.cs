using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MTFClientServerCommon.MTFAccessControl.UsbDrive;

namespace UsbDrive
{
    public static class AccessRequestSerializer
    {

        public static string Encrypt(AccessRequest Request)
        {
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Convert.FromBase64String("xYx4bk2XMZO4jkMbDYytIrynLshoSnSk1oBd0/RnM+c=");
                rijAlg.IV = Convert.FromBase64String("Vbcs3qk/4wpxDuvloS7H7w==");
                rijAlg.Padding = PaddingMode.Zeros;
                using (ICryptoTransform decryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV))
                {
                    using (var memoryStream = new MemoryStream())
                    {

                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                        {
                            IFormatter bin = new BinaryFormatter();
                            bin.Serialize(cryptoStream, Request);
                            cryptoStream.FlushFinalBlock();
                            return BitConverter.ToString(memoryStream.ToArray());
                        }
                    }
                }

            }
        }

        public static AccessRequest Decrypt(string cryptedText)
        {
            byte[] data = Array.ConvertAll<string, byte>(cryptedText.Split('-'), s => Convert.ToByte(s, 16));
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Convert.FromBase64String("xYx4bk2XMZO4jkMbDYytIrynLshoSnSk1oBd0/RnM+c=");
                rijAlg.IV = Convert.FromBase64String("Vbcs3qk/4wpxDuvloS7H7w==");
                rijAlg.Padding = PaddingMode.Zeros;
                using (ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();
                            IFormatter bin = new BinaryFormatter();
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return (AccessRequest)bin.Deserialize(memoryStream);
                        }
                    }
                }

            }
        }

        private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }


    }
}
