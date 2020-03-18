using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.Pipeline;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Example.NSBConfiguration
{
    public class KMSEncryptionService : IEncryptionService
    {
        private readonly IAmazonKeyManagementService _client;
        private string _encryptionKeyIdentifier;

        public KMSEncryptionService(string encryptionKeyIdentifier, IAmazonKeyManagementService client)
        {
            _encryptionKeyIdentifier = encryptionKeyIdentifier;
            _client = client;
        }

        public string Decrypt(EncryptedValue encryptedValue, IIncomingLogicalMessageContext context)
        {
            if (encryptedValue == null || String.IsNullOrEmpty(encryptedValue.EncryptedBase64Value))
                return null;

            if (!context.Headers.ContainsKey(EncryptionHeaders.RijndaelKeyIdentifier))
                return null;

            var decryptlabel = context.Headers[EncryptionHeaders.RijndaelKeyIdentifier];

            var decryptRequest = new DecryptRequest { KeyId = decryptlabel };
            var value = Convert.FromBase64String(encryptedValue.EncryptedBase64Value);
            decryptRequest.CiphertextBlob = new System.IO.MemoryStream(value);

            var response = _client.DecryptAsync(decryptRequest).GetAwaiter().GetResult();

            if(response != null)
            {
                return Encoding.UTF8.GetString(response.Plaintext.ToArray());
            }

            return null;
        }

        public EncryptedValue Encrypt(string value, IOutgoingLogicalMessageContext context)
        {
                      
            var encrypted = _client.EncryptAsync(new EncryptRequest
                { KeyId = _encryptionKeyIdentifier, 
                    Plaintext = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(value)) 
                } ).GetAwaiter().GetResult();

            string base64Value = encrypted != null ? Convert.ToBase64String(encrypted.CiphertextBlob.ToArray())  : null;

            return new EncryptedValue
            {
                EncryptedBase64Value = base64Value
            };
        }

        protected internal void AddKeyIdentifierHeader(IOutgoingLogicalMessageContext context)
        {
            context.Headers[EncryptionHeaders.RijndaelKeyIdentifier] = _encryptionKeyIdentifier;
        }
    }
}
