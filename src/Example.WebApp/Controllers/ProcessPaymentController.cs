using System;
using System.Threading.Tasks;
using Example.PaymentSaga.Contracts.Commands;
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
            await messageSession.Send(message);
            return "Message sent to endpoint";
        }
    }
}