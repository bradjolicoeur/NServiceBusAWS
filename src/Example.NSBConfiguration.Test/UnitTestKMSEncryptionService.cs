using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Amazon.KeyManagementService;

namespace Example.NSBConfiguration.Test
{
    [TestClass]
    public class UnitTestKMSEncryptionService
    {
      
        [TestMethod]
        public void EncryptString()
        {
            const string ENCRYPT_VALUE = "This is a test value";
            var sut = new KMSEncryptionService("bc436485-5092-42b8-92a3-0aa8b93536dc", new AmazonKeyManagementServiceConfig { UseHttp = true, ServiceURL = "http://localhost:8081" });

            var value = sut.Encrypt(ENCRYPT_VALUE, null);

            value.Should().NotBeNull();
            value.EncryptedBase64Value.Should().NotBeNullOrEmpty();

            //var decrypted = sut.Decrypt(value, null);

            //value.Should().NotBeNull();
            //value.Should().BeEquivalentTo(ENCRYPT_VALUE);
        }
    }
}
