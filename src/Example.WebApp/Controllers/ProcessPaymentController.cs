using System;
using System.Threading.Tasks;
using Example.PaymentSaga.Contracts.Commands;
using Example.PaymentSaga.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NServiceBus;

namespace Example.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessPaymentController : ControllerBase
    {
        IMessageSession messageSession;

        public ProcessPaymentController(IMessageSession messageSession)
        {
            this.messageSession = messageSession;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var message = new ProcessPayment { ReferenceId = Guid.NewGuid().ToString() , AccountNumberEncrypted = "123456", RoutingNumber = "555555", Amount = 100.45M, RequestDate = DateTime.UtcNow};
            var response = await messageSession.Request<ProcessPaymentReply>(message);
            return "Payment Status: " + JsonConvert.SerializeObject(response);
        }
    }
}