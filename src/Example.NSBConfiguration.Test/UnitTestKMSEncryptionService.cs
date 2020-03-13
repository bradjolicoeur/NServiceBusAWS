using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace Example.NSBConfiguration.Test
{
    [TestClass]
    public class UnitTestKMSEncryptionService
    {
        [Ignore]
        [TestMethod]
        public void EncryptString()
        {
            const string ENCRYPT_VALUE = "This is a test value";
            var sut = new KMSEncryptionService();

            var value = sut.Encrypt(ENCRYPT_VALUE, null);

            value.Should().NotBeNull();

            var decrypted = sut.Decrypt(value, null);

            value.Should().NotBeNull();
            value.Should().BeEquivalentTo(ENCRYPT_VALUE);
        }
    }
}
