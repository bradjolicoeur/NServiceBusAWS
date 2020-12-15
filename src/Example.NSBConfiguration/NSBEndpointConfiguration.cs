using NServiceBus;
using Autofac;
using Amazon.SQS;
using Amazon.Runtime;
using Amazon.S3;
using Example.PaymentSaga.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Commands;
using Example.PaymentProcessor.Contracts.Events;
using System;
using NServiceBus.Encryption.MessageProperty;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Amazon.KeyManagementService;

namespace Example.NSBConfiguration
{
    public static class NSBEndpointConfiguration
    {

        public static EndpointConfiguration ConfigureEndpoint(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);

            //Configure the transport option, this can be easily swapped for other queue providers
            //https://docs.particular.net/transports/
            var transport = ConfigureTransport(endpointConfiguration);

            //configure the persistance; for subscriptions, sagas, deferrals, timeouts, delayed retries and outbox 
            //https://docs.particular.net/persistence/
            var persistance = ConfigurePersistance(endpointConfiguration);

            //This routes messages to endpoints and configures subscriptions for events
            //https://docs.particular.net/nservicebus/messaging/routing
            ConfigureMessageRouting(transport);

            //This creates the queues if they do not exist as well as any db objects requried
            endpointConfiguration.EnableInstallers();

            //Conventions are used to identify messages 
            ConfigureConventions(endpointConfiguration);

            //configure the property level encryption convention
            //https://docs.particular.net/nservicebus/security/property-encryption
            ConfigurePropertyEncryption(endpointConfiguration);

            //This is the format of the message payload, default is xml, however json is a smaller payload size
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            return endpointConfiguration;

        }

        private static void ConfigurePropertyEncryption(EndpointConfiguration endpointConfiguration)
        {
            //this is a key stored in KMS; for reference this same value is in the init\seed.yaml file for kms simulator config
            var defaultKey = "bc436485-5092-42b8-92a3-0aa8b93536dc"; //Todo: should not be hardcoded

            var kmsClient = new AmazonKeyManagementServiceClient(new AmazonKeyManagementServiceConfig
            {
                //This is for localstack and is not needed if targeting AWS directly
                UseHttp = true,
                ServiceURL = "http://localhost:8081"
            });

            var kmsEncryptionService = new KMSEncryptionService(defaultKey, kmsClient); 

            endpointConfiguration.EnableMessagePropertyEncryption(
                encryptionService: kmsEncryptionService,
                encryptedPropertyConvention: propertyInfo =>
                {
                    return propertyInfo.Name.EndsWith("Encrypted")||propertyInfo.Name.EndsWith("AccountNumber");
                }
            );
        }

        private static PersistenceExtensions ConfigurePersistance(EndpointConfiguration endpointConfiguration)
        {
            //todo: put this connection string in an environment variable
            var connection = "server=localhost;user=devuser;database=testdb;port=3306;password=TestPassword;AllowUserVariables=True;AutoEnlist=false";
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.CacheFor(TimeSpan.FromMinutes(1));
            persistence.SqlDialect<SqlDialect.MySql>();
            persistence.ConnectionBuilder(() => new MySqlConnection(connection));

            return persistence;
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

        private static void ConfigureMessageRouting(TransportExtensions<SqsTransport> transport)
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

        private static TransportExtensions<SqsTransport> ConfigureTransport(EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            //configure client factory for localstack...not needed for normal AWS
            transport.ClientFactory(() => new AmazonSQSClient(
                new EnvironmentVariablesAWSCredentials(),
                new AmazonSQSConfig //for localstack
                {
                    ServiceURL = "http://localhost:4566",
                    UseHttp = true,
                }));

            // S3 bucket only required for messages larger than 256KB
            var s3Configuration = transport.S3("transportbucket", "my/key/prefix");

            //configure client factory for localstack...not needed for normal AWS
            s3Configuration.ClientFactory(() =>
                new AmazonS3Client(new EnvironmentVariablesAWSCredentials()
                , new AmazonS3Config
                {
                    ServiceURL = "http://localhost:4566",
                    ForcePathStyle = true,
                    UseHttp = true,
                }));


            return transport;
        }
    }
}
