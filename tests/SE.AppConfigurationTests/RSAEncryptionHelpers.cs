using SE.AppConfiguration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SE.AppConfigurationTests
{
    public class RSAEncryptionHelpers
    {
        private RSAParameters publicKey;
        private RSAParameters privateKey;

        public void AssignNewKey()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                publicKey = rsa.ExportParameters(false);
                privateKey = rsa.ExportParameters(true);
            }
        }

        /// <summary>
        /// Encrypts an input string using AES with a 256 bits key in GCM authenticated mode.
        /// </summary>
        /// <param name="text">The plain string input to encrypt.</param>
        /// <param name="key">The encryption key being used.</param>
        /// <returns>The base64 encoded output string encrypted with AES.</returns>
        //public string EncryptData(string text)
        //{
        //    byte[] result;

        //    if (string.IsNullOrEmpty(text))
        //        throw new ArgumentNullException(nameof(text));

        //    using (var rsa = new RSACryptoServiceProvider())
        //    {
        //        rsa.PersistKeyInCsp = false;
        //        rsa.ImportParameters(publicKey);

        //        result = rsa.Encrypt(text.ToByteArray(), true);
        //    }
        //    return Convert.ToBase64String(result);
        //}

        public byte[] EncryptData(byte[] byteToEncrypt)
        {
            byte[] cipherbytes;

            // No need to specify key size in constructor when importing a key.
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(publicKey);

                cipherbytes = rsa.Encrypt(byteToEncrypt, true);
            }

            return cipherbytes;
        }


        //public string EncryptData<TSettings>(TSettings settings)
        //where TSettings : class, new()
        //{
        //    byte[] key;

        //    if (settings == null)
        //        throw new ArgumentNullException(nameof(settings));

        //    using (var rsa = new RSACryptoServiceProvider())
        //    {
        //        rsa.PersistKeyInCsp = false;
        //        rsa.ImportParameters(publicKey);
        //        var json = JsonSerializer.Serialize<TSettings>(settings);
        //        var jsonBytes = Encoding.UTF8.GetBytes(json);

        //        key = rsa.Encrypt(jsonBytes, true);
        //    }
        //    return Convert.ToBase64String(key);
        //}

        public byte[] DecryptData(byte[] byteToDecrypt)
        {
            byte[] plain;

            // No need to specify key size in constructor when importing a key.
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;

                rsa.ImportParameters(privateKey);
                plain = rsa.Decrypt(byteToDecrypt, true);
            }

            return plain;
        }

        public string DecryptData(string stringToDecrypt)
        {
            if (string.IsNullOrEmpty(stringToDecrypt))
                throw new ArgumentNullException(nameof(stringToDecrypt));

            string plain;

            // No need to specify key size in constructor when importing a key.
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(privateKey);
                plain = Convert.ToBase64String(rsa.Decrypt(stringToDecrypt.ToByteArray(), true));
            }

            return plain;
        }

    }
}