using System;
using System.Text;
using Xunit;

namespace SE.AppConfigurationTests
{
    public class EncryptionTests
    {
        private readonly AppSettings settings;
        private readonly RSAEncryptionHelpers key;

        public EncryptionTests()
        {
            settings = new AppSettings { ConnectionString = "pgsqlconnectionstring", EmailApiKey = "api1.538c9073e4eb4461a87f1947bd47adb2bdd3a53bb26d4daf81d1e21b2039aab1" };
            key = new RSAEncryptionHelpers();
            key.AssignNewKey();
        }
        [Fact]
        public void Encrypt_When_GivenString_Data()
        {
            var encryptedParams = key.EncryptData(Encoding.UTF8.GetBytes(settings.ConnectionString));
            var decryptedParams = key.DecryptData(encryptedParams);

            Assert.NotNull(encryptedParams);
        }

        [Fact]
        public void Decrypt_When_GivenSettingsAndKey_Then_DecryptedJson()
        {
            //var apiKey = "api1.538c9073e4eb4461a87f1947bd47adb2bdd3a53bb26d4daf81d1e21b2039aab1";
            var connectionString = "pgsqlconnectionstring";
            //var encrypted = key.EncryptData<AppSettings>(settings);
            var encrypted = key.EncryptData(Encoding.UTF8.GetBytes(settings.ConnectionString));
            var decrypted = key.DecryptData(encrypted);

            //var result = JsonSerializer.Deserialize<AppSettings>(decrypted);
            var result = Convert.ToBase64String(decrypted);

            Assert.NotNull(encrypted);
            //Assert.Equal(apiKey, result.EmailApiKey);
            Assert.Equal(connectionString, result);
        }


    }
}
