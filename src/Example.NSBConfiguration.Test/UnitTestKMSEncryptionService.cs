using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Amazon.KeyManagementService;
using NServiceBus.Pipeline;
using Moq;
using NServiceBus.Encryption.MessageProperty;
using System.Collections.Generic;

namespace Example.NSBConfiguration.Test
{
    [TestClass]
    public class UnitTestKMSEncryptionService
    {
      
        [TestMethod]
        public void EncryptString()
        {
            const string KEY_ID = "bc436485-5092-42b8-92a3-0aa8b93536dc";
            var mockContext = new Mock<IIncomingLogicalMessageContext>();
            var headers = new Dictionary<string, string>
            {
                { EncryptionHeaders.RijndaelKeyIdentifier, KEY_ID }
            };
            mockContext.SetupGet(p => p.Headers)
                .Returns(headers);

            const string ENCRYPT_VALUE = "This is a test value";
            var sut = new KMSEncryptionService(KEY_ID
                , new AmazonKeyManagementServiceConfig { UseHttp = true, ServiceURL = "http://localhost:8081" });

            var value = sut.Encrypt(ENCRYPT_VALUE, null);

            value.Should().NotBeNull();
            value.EncryptedBase64Value.Should().NotBeNullOrEmpty();

            var decrypted = sut.Decrypt(value, mockContext.Object);

            value.Should().NotBeNull();
            decrypted.Should().BeEquivalentTo(ENCRYPT_VALUE);
        }
    }
}
