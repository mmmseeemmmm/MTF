using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    public static class SecurityHelper
    {
        static readonly string PasswordHash = "uvzrJLMd4AE2Bpgm";
        static readonly string SaltKey = "L6yJ8w9VaQrLjdHs";
        static readonly string VIKey = "8pLMmzYcfncmhhXG";

        static readonly ICryptoTransform encryptor;
        static readonly ICryptoTransform decryptor;

        static SecurityHelper()
        {
            var keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
        }

        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                return null;
            }

            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherTextBytes = null;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memoryStream.ToArray();
                        cryptoStream.Close();
                    }
                    memoryStream.Close();
                }
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch
            {
                return null;
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }

            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];

                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            }
            catch 
            {
                return null;
            }
        }


        public static void DecryptData(List<Person> persons)
        {
            if (persons != null)
            {
                foreach (var person in persons)
                {
                    person.ANETId = Decrypt(person.ANETId);
                }
            }
        }

        public static void EncryptData(List<Person> persons)
        {
            if (persons != null)
            {
                foreach (var person in persons)
                {
                    person.ANETId = Encrypt(person.ANETId);
                }
            }
        }
    }
}
