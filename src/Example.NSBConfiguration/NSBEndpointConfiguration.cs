using NServiceBus;
using Autofac;
using Amazon.SQS;
using Amazon.Runtime;
using Amazon.S3;
using Example.PaymentSaga.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Events;

namespace Example.NSBConfiguration
{
    public static class NSBEndpointConfiguration
    {

        public static EndpointConfiguration ConfigureEndpoint(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            var transport = ConfigureLocalStackTransport(endpointConfiguration);
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            ConfigureRouting(transport);

            endpointConfiguration.EnableInstallers();

            ConfigureConventions(endpointConfiguration);

            return endpointConfiguration;

        }
        public static EndpointConfiguration ConfigureEndpoint(ContainerBuilder builder, string endpointName)
        {
            var endpointConfiguration = ConfigureEndpoint(endpointName);

            IEndpointInstance endpoint = null;
            builder.Register(x => endpoint)
                .As<IEndpointInstance>()
                .SingleInstance();

            var container = builder.Build();

            endpointConfiguration.UseContainer<AutofacBuilder>(
                customizations: customizations =>
                {
                    customizations.ExistingLifetimeScope(container);
                });

            return endpointConfiguration;
        }

        private static void ConfigureRouting(TransportExtensions<SqsTransport> transport)
        {
            var routing = transport.Routing();
            routing.RouteToEndpoint(
                       messageType: typeof(ProcessPayment)
                       , destination: "Example.PaymentSaga"
                       );

            routing.RouteToEndpoint(
                       messageType: typeof(MakePayment)
                       , destination: "Example.PaymentProcessorWorker"
                       );

            routing.RegisterPublisher(
                        eventType: typeof(ICompletedMakePayment),
                        publisherEndpoint: "Example.PaymentProcessorWorker");
        }

        private static void ConfigureConventions(EndpointConfiguration endpointConfiguration)
        {
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(
                type =>
                {
                    return type.Namespace.EndsWith("Commands");
                });
            conventions.DefiningEventsAs(
                type =>
                {
                    return type.Namespace.EndsWith("Events");
                });
            conventions.DefiningMessagesAs(
                type =>
                {
                    return type.Namespace.EndsWith("Messages");
                });
        }

        private static TransportExtensions<SqsTransport> ConfigureLocalStackTransport(EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            //configure client factory for localstack...not needed for normal AWS
            transport.ClientFactory(() => new AmazonSQSClient(
                new EnvironmentVariablesAWSCredentials(),
                new AmazonSQSConfig //for localstack
                {
                    ServiceURL = "http://localhost:4576",
                    UseHttp = true,
                }));

            // S3 bucket only required for messages larger than 256KB
            var s3Configuration = transport.S3("transportbucket", "my/key/prefix");

            //configure client factory for localstack...not needed for normal AWS
            s3Configuration.ClientFactory(() =>
                new AmazonS3Client(new EnvironmentVariablesAWSCredentials()
                , new AmazonS3Config
                {
                    ServiceURL = "http://localhost:4572",
                    ForcePathStyle = true,
                    UseHttp = true,
                }));


            return transport;
        }
    }
}
