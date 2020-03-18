
using System;

namespace Example.PaymentProcessor.Contracts.Commands
{
    public class MakePayment
    {
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumberEncrypted { get; set; }
        public string RoutingNumber { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
