using System;
using System.Threading.Tasks;
using Xunit;
using Example.ConsumerWorker.Messages.Commands;
using Example.ConsumerWorker.Handlers;
using NServiceBus.Testing;


namespace Example.ConsumerWorker.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task ShouldReplyWithResponseMessage()
        {
            var handler = new HandleFirstCommand();
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new FirstCommand(), context)
                .ConfigureAwait(false);

            Assert.Empty(context.SentMessages);
            //Assert.IsInstanceOf<MyResponse>(context.RepliedMessages[0].Message);
        }
    }
}
