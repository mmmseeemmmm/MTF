using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace MTFClientServerCommon.MTFAccessControl.UsbDrive
{
    public static class AccessKeySerializer
    {
        public static byte[] EncryptAccessKeyToArray(AccessKey Key, string privateKeyForSignBase64)
        {
            var privateKeyBase64 = "uc1rDSGOjrQPiBtL3syVIMcO/7ne1knTZYyEDOCqCyCzUvXGkoxWn44cglNWwV/+XifeZZhvqDhoFYrjeVqtYzmmPO6uJCJX39fpd0fTXzAWKbANHOvRXBOnaG7wFX40kQ3KprbnE+RzVUmT28XCw72aaSFm11BRh6Ofu35qIIk=";

            // Check arguments.
            if (Key == null)
                throw new ArgumentNullException("AccessKey is null");

            Key.Signature = CreateSignature(Key.GetTesxtForSign(), privateKeyForSignBase64);

            // Create the streams used for encryption.
            using (var ms = new MemoryStream())
            using(System.IO.StreamWriter writer = new System.IO.StreamWriter(ms))
            {
                using (Rijndael rijAlg = Rijndael.Create())
                {
                    // Create an Rijndael object
                    // with the specified key and IV.
                    rijAlg.Key = Convert.FromBase64String("HEMSmWGLRqJ94WTCu9WxTHsUgj6kU1b4Zissvkdiqwk=");
                    rijAlg.IV = Convert.FromBase64String("4fiK32iMj0vLZd3JVxUPlA==");

                    // Create a encryptor to perform the stream transform.
                    using (ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            //Encrypt all data to the stream.
                            bin.Serialize(csEncrypt, Key);
                        }
                    }
                }

                return ms.ToArray();
            }
        }

        public static AccessKey DecryptAccessKeyFromFile(string FileName)
        {
            AccessKey key = null;
            // Create a decryptor to perform the stream transform.
            // Create the streams used for decryption.
            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                using (Rijndael rijAlg = Rijndael.Create())
                {
                    // Create an Rijndael object
                    // with the specified key and IV.
                    rijAlg.Key = Convert.FromBase64String("HEMSmWGLRqJ94WTCu9WxTHsUgj6kU1b4Zissvkdiqwk=");
                    rijAlg.IV = Convert.FromBase64String("4fiK32iMj0vLZd3JVxUPlA==");

                    // Create a decryptor to perform the stream transform.
                    using (ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            key = (AccessKey)bin.Deserialize(csDecrypt);
                        }
                    }
                }
            }

            return CheckSIgnature(key.GetTesxtForSign(), key.Signature) ? key : null;
        }

        private static byte[] CreateSignature(string messageToSign, string privateKey)
        {
            var privateKeys = privateKey.Split(new[] { ";" }, StringSplitOptions.None);
            if (privateKeys.Length != 6)
            {
                throw new Exception("Invalid length of private key");
            }

            string privateKeyD = privateKeys[0];
            string privateKeyDP = privateKeys[1];
            string privateKeyDQ = privateKeys[2];
            string privateKeyInverseQ = privateKeys[3];
            string privateKeyP = privateKeys[4];
            string privateKeyQ = privateKeys[5];

            var Modulus = "uc1rDSGOjrQPiBtL3syVIMcO/7ne1knTZYyEDOCqCyCzUvXGkoxWn44cglNWwV/+XifeZZhvqDhoFYrjeVqtYzmmPO6uJCJX39fpd0fTXzAWKbANHOvRXBOnaG7wFX40kQ3KprbnE+RzVUmT28XCw72aaSFm11BRh6Ofu35qIIk=";

            var converter = new ASCIIEncoding();
            byte[] plainText = converter.GetBytes(messageToSign);

            var rsaWrite = new RSACryptoServiceProvider();

            rsaWrite.ImportParameters(new RSAParameters
            {
                Exponent = new byte[] { 1, 0, 1 },
                Modulus = Convert.FromBase64String(Modulus),

                D = Convert.FromBase64String(privateKeyD),
                P = Convert.FromBase64String(privateKeyP),
                InverseQ = Convert.FromBase64String(privateKeyInverseQ),
                DQ = Convert.FromBase64String(privateKeyDQ),
                DP = Convert.FromBase64String(privateKeyDP),
                Q = Convert.FromBase64String(privateKeyQ)
            });


            return rsaWrite.SignData(plainText, new SHA1CryptoServiceProvider());
        }

        private static bool CheckSIgnature(string message, byte[] signature)
        {
            if (signature == null || string.IsNullOrEmpty(message))
            {
                return false;
            }

            var Modulus = "uc1rDSGOjrQPiBtL3syVIMcO/7ne1knTZYyEDOCqCyCzUvXGkoxWn44cglNWwV/+XifeZZhvqDhoFYrjeVqtYzmmPO6uJCJX39fpd0fTXzAWKbANHOvRXBOnaG7wFX40kQ3KprbnE+RzVUmT28XCw72aaSFm11BRh6Ofu35qIIk=";

            var converter = new ASCIIEncoding();
            byte[] plainText = converter.GetBytes(message);

            var rsaRead = new RSACryptoServiceProvider();
            rsaRead.ImportParameters(new RSAParameters
            {
                Exponent = new byte[] { 1, 0, 1 },
                Modulus = Convert.FromBase64String(Modulus)
            });

            return rsaRead.VerifyData(plainText, new SHA1CryptoServiceProvider(), signature);
        }
    }
}
