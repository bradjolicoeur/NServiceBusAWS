using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using Example.ConsumerWorker.Messages.Commands;
using System;


namespace Example.ConsumerWorker.Handlers
{

    public class HandleFirstCommand : IHandleMessages<FirstCommand>
    {

        //TODO: can we inject this instead?
        static ILog log = LogManager.GetLogger<HandleFirstCommand>();

        public Task Handle(FirstCommand message, IMessageHandlerContext context)
        {
            log.Info("Message Handled " + message.MessageValue);

            return Task.CompletedTask;
        }

    }
}