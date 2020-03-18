using Microsoft.VisualStudio.TestTools.UnitTesting;
using Example.PaymentSaga.Sagas;
using Example.PaymentSaga.Models;
using NServiceBus.Testing;
using Example.PaymentSaga.Mapping;
using AutoMapper;
using Example.PaymentSaga.Contracts.Commands;
using System;
using System.Threading.Tasks;
using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentSaga.Messages;

namespace Example.PaymentSaga.Tests
{
    [TestClass]
    public class SagaTest
    {
        
        [TestInitialize]
        public void SetUpFixture()
        {
            var config = new MapperConfiguration(opts =>
            {
                // Add your mapper profile configs or mappings here
                opts.AddProfile(new AutomapperProfile());
            });

            Mapper = config.CreateMapper();
        }

        private static IMapper Mapper;

        [TestMethod]
        public async Task InitializeSaga()
        {
            //Arrange
            var sagaData = new ProcessPaymentData();

            var saga = new ProcessPaymentSaga(Mapper)
            {
                Data = sagaData
            };
            var context = new TestableMessageHandlerContext();

            var message = new ProcessPayment { 
                ReferenceId = Guid.NewGuid().ToString()
                , AccountNumberEncrypted = "123456"
                , RoutingNumber = "555555"
                , Amount = 100.45M
                , RequestDate = DateTime.UtcNow };

            //Act
            await saga.Handle(message, context);

            //Assert
            Assert.AreEqual(message.AccountNumberEncrypted, sagaData.AccountNumberEncrypted);
            Assert.AreEqual(message.RoutingNumber, sagaData.RoutingNumber);
            Assert.AreEqual(message.Amount, sagaData.Amount);
            Assert.AreEqual(message.RequestDate, sagaData.RequestDate);

            var makePayment = (MakePayment)context.SentMessages[0].Message;

            Assert.AreEqual(message.AccountNumberEncrypted, makePayment.AccountNumberEncrypted);
            Assert.AreEqual(message.RoutingNumber, makePayment.RoutingNumber);
            Assert.AreEqual(message.Amount, makePayment.Amount);
            Assert.AreEqual(message.RequestDate, makePayment.RequestDate);



            var timeout = (ProcessPaymentTimeout)context.SentMessages[1].Message;
            Assert.IsNotNull(timeout);
        }
    }
}
