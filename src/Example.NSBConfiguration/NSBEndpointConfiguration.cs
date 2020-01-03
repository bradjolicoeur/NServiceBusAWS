using System;
using NServiceBus;
using Autofac;
using Example.ConsumerWorker.Messages.Commands;
using Amazon.SQS;
using Amazon.Runtime;
using Amazon.S3;

namespace Example.NSBConfiguration
{
    public static class NSBEndpointConfiguration
    {
        private static readonly string TransportConfiguration = Environment.GetEnvironmentVariable("TRANSPORT_CONFIGURATION");

        public static EndpointConfiguration ConfigureEndpoint(ContainerBuilder builder, string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);

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

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            TransportExtensions transport = null;

            if (TransportConfiguration.Equals("LearningTransport"))
            {
                transport = ConfigureLearningTransport(endpointConfiguration);
                endpointConfiguration.UsePersistence<LearningPersistence>();
            }
            else
            {
                transport = ConfigureLocalStackTransport(endpointConfiguration);
                endpointConfiguration.UsePersistence<InMemoryPersistence>();
            }

            ConfigureMessageRouting(transport.Routing());

            endpointConfiguration.EnableInstallers();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(
                type =>
                {
                    return type.Namespace.EndsWith("Messages.Commands");
                });

            return endpointConfiguration;
        }

        private static void ConfigureMessageRouting(RoutingSettings routing)
        {
            routing.RouteToEndpoint(
                        messageType: typeof(FirstCommand)
                        , destination: "example.consumerworker"
                        );
        }

        private static TransportExtensions ConfigureLearningTransport(EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            return transport;
        }

        private static TransportExtensions ConfigureLocalStackTransport(EndpointConfiguration endpointConfiguration)
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
            var s3Configuration = transport.S3("myBucketName", "my/key/prefix");

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
