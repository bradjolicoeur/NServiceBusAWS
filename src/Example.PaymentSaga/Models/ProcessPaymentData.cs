using NServiceBus;

namespace Example.PaymentSaga.Models
{
    public class ProcessPaymentData : ContainSagaData
    {
        public string ReferenceId { get; set; }
    }
}
