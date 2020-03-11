using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.Pipeline;

namespace Example.NSBConfiguration
{
    public class KMSEncryptionService : IEncryptionService
    {
        public string Decrypt(EncryptedValue encryptedValue, IIncomingLogicalMessageContext context)
        {
            throw new NotImplementedException();
        }

        public EncryptedValue Encrypt(string value, IOutgoingLogicalMessageContext context)
        {
            throw new NotImplementedException();
        }
    }
}
