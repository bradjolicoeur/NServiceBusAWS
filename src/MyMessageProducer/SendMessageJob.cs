using NServiceBus;
using NServiceBus.Logging;
using MyMessages.Commands;
using FluentScheduler;
using System;

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
        var message = new MyMessage{MessageValue = Guid.NewGuid().ToString()};

        log.Info("Send Message " + message.MessageValue);

        endpoint.Send(message)
            .GetAwaiter().GetResult();
    }
}