using Example.PaymentSaga.Contracts.Messages;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;

namespace Example.WebApp.Handlers
{
    public class HandleProcessPaymentReply : IHandleMessages<ProcessPaymentReply>
    {
        static ILog log = LogManager.GetLogger<HandleProcessPaymentReply>();

        public Task Handle(ProcessPaymentReply message, IMessageHandlerContext context)
        {
            //This is where you might put a webhook post to the caller, write to SNS topic or push notification via signalr
            //This can address the situation where the immediate response was pending
            log.Info("Handled ProcessPaymentReply " + message.ReferenceId + " " + message.Status +" webhook post");

            return Task.CompletedTask;
        }
    }
}
