using Example.PaymentSaga.Models;
using System;
using NServiceBus;
using Example.PaymentSaga.Contracts.Commands;
using System.Threading.Tasks;
using Example.PaymentSaga.Messages;
using NServiceBus.Logging;
using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Events;
using Example.PaymentSaga.Contracts.Messages;

namespace Example.PaymentSaga.Sagas
{
    public class ProcessPaymentSaga : Saga<ProcessPaymentData>,
        IAmStartedByMessages<ProcessPayment>,
        IHandleMessages<ICompletedMakePayment>,
        IHandleTimeouts<ProcessPaymentTimeout>
    {

        static ILog log = LogManager.GetLogger<ProcessPaymentSaga>();

        public async Task Handle(ProcessPayment message, IMessageHandlerContext context)
        {
            log.Info("Start Saga for " + Data.ReferenceId);

            await context.Send(new MakePayment { ReferenceId = Data.ReferenceId });

            await RequestTimeout<ProcessPaymentTimeout>(context, TimeSpan.FromMinutes(3));
        }

        public async Task Handle(ICompletedMakePayment message, IMessageHandlerContext context)
        {
            log.Info("Handle ICompletedMakePayment " + message.ReferenceId);

            await ReplyToOriginator(context, new ProcessPaymentReply { ReferenceId = Data.ReferenceId });
        }

        public Task Timeout(ProcessPaymentTimeout state, IMessageHandlerContext context)
        {
            log.Info("Handle Timeout for " + Data.ReferenceId);

            throw new NotImplementedException();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ProcessPaymentData> mapper)
        {
            mapper.ConfigureMapping<ProcessPayment>(message => message.ReferenceId)
            .ToSaga(sagaData => sagaData.ReferenceId);

            mapper.ConfigureMapping<ICompletedMakePayment>(message => message.ReferenceId)
            .ToSaga(sagaData => sagaData.ReferenceId);
        }
    }
}
