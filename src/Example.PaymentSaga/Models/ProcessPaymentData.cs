using NServiceBus;
using System;

namespace Example.PaymentSaga.Models
{
    public class ProcessPaymentData : ContainSagaData
    {
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string ConfirmationId { get; set; }
    }
}
