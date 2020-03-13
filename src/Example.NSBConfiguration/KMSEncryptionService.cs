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
        private readonly AmazonKeyManagementServiceClient _client;
        private string _encryptionKeyIdentifier;

        public KMSEncryptionService(string encryptionKeyIdentifier, AmazonKeyManagementServiceConfig config)
        {
            _encryptionKeyIdentifier = encryptionKeyIdentifier;
            _client = new AmazonKeyManagementServiceClient(config);

        }


        public string Decrypt(EncryptedValue encryptedValue, IIncomingLogicalMessageContext context)
        {
            var aliasResponse = _client.ListAliasesAsync(new ListAliasesRequest { Limit = 1000 }).GetAwaiter().GetResult();

            throw new NotImplementedException();

            //StreamReader reader = new StreamReader(stream);
            //string text = reader.ReadToEnd();
        }

        public EncryptedValue Encrypt(string value, IOutgoingLogicalMessageContext context)
        {
                      
            var encrypted = _client.EncryptAsync(new EncryptRequest
                { KeyId = _encryptionKeyIdentifier, 
                    Plaintext = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(value)) 
                } ).GetAwaiter().GetResult();

            return new EncryptedValue
            {
                EncryptedBase64Value = Convert.ToBase64String(encrypted.CiphertextBlob.ToArray())
            };
        }

        protected internal void AddKeyIdentifierHeader(IOutgoingLogicalMessageContext context)
        {
            context.Headers[EncryptionHeaders.RijndaelKeyIdentifier] = _encryptionKeyIdentifier;
        }
    }
}
