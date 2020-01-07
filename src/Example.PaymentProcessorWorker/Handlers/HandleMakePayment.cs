using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace Example.PaymentProcessorWorker.Handlers
{
    public class HandleMakePayment : IHandleMessages<MakePayment>
    {
        static ILog log = LogManager.GetLogger<HandleMakePayment>();

        public async Task Handle(MakePayment message, IMessageHandlerContext context)
        {

            log.Info("Processing Payment " + message.ReferenceId);

            await context.Publish<ICompletedMakePayment>(messageConstructor:
                m =>
                 {
                     m.ReferenceId = message.ReferenceId;
                     m.ConfirmationId = Guid.NewGuid().ToString();
                     m.Status = "Approved";
                     m.StatusDate = DateTime.UtcNow;
                 });
        }
    }
}
