using System;
using System.Threading.Tasks;
using Example.PaymentSaga.Contracts.Commands;
using Example.PaymentSaga.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
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
            var message = new ProcessPayment { ReferenceId = Guid.NewGuid().ToString() };
            var response = await messageSession.Request<ProcessPaymentReply>(message);
            return "Completed processing payment" + response.ReferenceId;
        }
    }
}