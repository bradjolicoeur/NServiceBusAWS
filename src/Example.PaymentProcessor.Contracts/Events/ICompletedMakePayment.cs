
using System;

namespace Example.PaymentProcessor.Contracts.Events
{
    public interface ICompletedMakePayment
    {
        string ReferenceId { get; set; }
        string ConfirmationId { get; set; }
        string Status { get; set; }
        DateTime StatusDate { get; set; }
    }
}
