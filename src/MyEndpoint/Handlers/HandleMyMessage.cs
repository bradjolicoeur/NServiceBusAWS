using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using MyMessages.Commands;
using System;


namespace MyEndpoint.Handlers
{

    public class HandleMyMessage : IHandleMessages<MyMessage>
    {

        //TODO: can we inject this instead?
        static ILog log = LogManager.GetLogger<HandleMyMessage>();

        public Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            log.Info("Message Handled " + message.MessageValue);

            return Task.CompletedTask;
        }

    }
}