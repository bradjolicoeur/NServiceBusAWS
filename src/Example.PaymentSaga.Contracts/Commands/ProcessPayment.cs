using System;

namespace Example.PaymentSaga.Contracts.Commands
{
    public class ProcessPayment
    {
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumberEncrypted { get; set; }
        public string RoutingNumber { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
