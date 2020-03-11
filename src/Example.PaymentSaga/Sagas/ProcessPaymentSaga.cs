using Example.PaymentSaga.Models;
using System;
using NServiceBus;
using mpe = NServiceBus.Encryption.MessageProperty;
using Example.PaymentSaga.Contracts.Commands;
using System.Threading.Tasks;
using Example.PaymentSaga.Messages;
using NServiceBus.Logging;
using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Events;
using Example.PaymentSaga.Contracts.Messages;
using AutoMapper;

namespace Example.PaymentSaga.Sagas
{
    public class ProcessPaymentSaga : Saga<ProcessPaymentData>,
        IAmStartedByMessages<ProcessPayment>,
        IHandleMessages<ICompletedMakePayment>,
        IHandleTimeouts<ProcessPaymentTimeout>
    {

        static ILog log = LogManager.GetLogger<ProcessPaymentSaga>();

        private IMapper Mapper { get; set; }

        public ProcessPaymentSaga(IMapper mapper)
        {
            Mapper = mapper;
        }

        public async Task Handle(ProcessPayment message, IMessageHandlerContext context)
        {
            log.Info("Start Saga for " + Data.ReferenceId);

            log.Info("Account:" + message.AccountNumberEncrypted);

            //update saga data
            Mapper.Map(message, Data);

            //Send command
            await context.Send(Mapper.Map<MakePayment>(Data));

            //Set timeout
            await RequestTimeout<ProcessPaymentTimeout>(context, TimeSpan.FromSeconds(25));
        }

        public async Task Handle(ICompletedMakePayment message, IMessageHandlerContext context)
        {
            log.Info("Handle ICompletedMakePayment " + message.ReferenceId);

            //update saga
            Mapper.Map(message, Data);

            await ReplyToOriginator(context, Mapper.Map<ProcessPaymentReply>(Data));
        }

        public async Task Timeout(ProcessPaymentTimeout state, IMessageHandlerContext context)
        {

            if (String.IsNullOrEmpty(Data.Status))
            {
                log.Info("Handle Timeout for " + Data.ReferenceId);

                var reply = Mapper.Map<ProcessPaymentReply>(Data);
                reply.Status = "Pending";
                reply.StatusDate = DateTime.UtcNow;

                await ReplyToOriginator(context, reply);
            }

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
