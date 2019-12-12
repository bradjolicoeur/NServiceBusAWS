using NServiceBus;
using NServiceBus.Logging;
using FluentScheduler;
using System;
using Example.ConsumerWorker.Messages.Commands;

namespace Example.ProducerWorker.Jobs
{
    public class SendMessageJob : IJob
    {
        IEndpointInstance endpoint;

        static ILog log = LogManager.GetLogger<SendMessageJob>();

        public SendMessageJob(IEndpointInstance endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Execute()
        {
            var message = new FirstCommand{MessageValue = Guid.NewGuid().ToString()};

            log.Info("Send Message " + message.MessageValue);

            endpoint.Send(message)
                .GetAwaiter().GetResult();
        }
    }
    
}