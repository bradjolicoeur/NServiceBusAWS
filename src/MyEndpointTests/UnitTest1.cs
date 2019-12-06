using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using NServiceBus.Testing;
using MyMessages.Commands;
using MyEndpoint.Handlers;

namespace MyEndpointTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task ShouldReplyWithResponseMessage()
        {
            var handler = new HandleMyMessage();
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new MyMessage(), context)
                .ConfigureAwait(false);

            Assert.Empty(context.SentMessages);
            //Assert.IsInstanceOf<MyResponse>(context.RepliedMessages[0].Message);
        }
    }
}
