using System;

namespace Example.PaymentSaga.Contracts.Messages
{
    public class ProcessPaymentReply
    {
        public string ReferenceId { get; set; }
        public string ConfirmationId { get; set; }
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
    }
}
