using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Amazon.KeyManagementService;
using NServiceBus.Pipeline;
using Moq;
using NServiceBus.Encryption.MessageProperty;
using System.Collections.Generic;
using Amazon.KeyManagementService.Model;
using System.Text;
using System;

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
            const string FAKE_ENCRYPTION = "FAKEENCRYPTEDVALUE";

            var mockClient = new Mock<IAmazonKeyManagementService>();
            mockClient.Setup(m => m.EncryptAsync(It.IsAny<EncryptRequest>(), default))
                .ReturnsAsync(new EncryptResponse {
                    CiphertextBlob = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(FAKE_ENCRYPTION)),
                    KeyId = KEY_ID
                });

            var sut = new KMSEncryptionService(KEY_ID, mockClient.Object);

            var value = sut.Encrypt(ENCRYPT_VALUE, null);

            value.Should().NotBeNull();
            value.EncryptedBase64Value.Should().NotBeNullOrEmpty();

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value.EncryptedBase64Value));
            decoded.Should().BeEquivalentTo(FAKE_ENCRYPTION);
        }

        [TestMethod]
        public void DecryptString()
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
            const string FAKE_ENCRYPTION = "FAKEENCRYPTEDVALUE";
            var base64Encrypted = Convert.ToBase64String(
                new System.IO.MemoryStream(Encoding.UTF8.GetBytes(FAKE_ENCRYPTION)).ToArray());

            var mockClient = new Mock<IAmazonKeyManagementService>();
            mockClient.Setup(m => m.DecryptAsync(It.IsAny<DecryptRequest>(), default))
                .ReturnsAsync(new DecryptResponse
                {
                    Plaintext = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(ENCRYPT_VALUE)),
                    KeyId = KEY_ID
                });

            var encryptedValue = new EncryptedValue
            {
                EncryptedBase64Value = base64Encrypted
            };

            var sut = new KMSEncryptionService(KEY_ID, mockClient.Object);

            var decrypted = sut.Decrypt(encryptedValue, mockContext.Object);

            decrypted.Should().NotBeNull();
            decrypted.Should().BeEquivalentTo(ENCRYPT_VALUE);
        }
    }
}
