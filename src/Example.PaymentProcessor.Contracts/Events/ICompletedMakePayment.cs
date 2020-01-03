
namespace Example.PaymentProcessor.Contracts.Events
{
    public interface ICompletedMakePayment
    {
        string ReferenceId { get; set; }
    }
}
