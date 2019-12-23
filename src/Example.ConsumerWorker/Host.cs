using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Autofac;
using Amazon.SQS;
using Amazon.Runtime;
using Amazon.S3;
using NServiceBus.Features;

namespace Example.ConsumerWorker
{
    class Host
    {
        // TODO: optionally choose a custom logging library
        // https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        static readonly ILog log = LogManager.GetLogger<Host>();

        IEndpointInstance endpoint = null;

        // TODO: give the endpoint an appropriate name
        public string EndpointName => "example.consumerworker";

        public async Task Start()
        {
            try
            {
                var endpointConfiguration = new EndpointConfiguration(EndpointName);

                var builder = new ContainerBuilder();

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

                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                //var transportExtensions = endpointConfiguration.UseTransport<LearningTransport>();

                var transport = endpointConfiguration.UseTransport<SqsTransport>();
                transport.ClientFactory(() => new AmazonSQSClient(new EnvironmentVariablesAWSCredentials(),
                    new AmazonSQSConfig //for localstack
                    {
                        ServiceURL = "http://localhost:4576",
                        UseHttp = true,
                    }));

                // S3 bucket only required for messages larger than 256KB
                var s3Configuration = transport.S3("myBucketName", "my/key/prefix");
                s3Configuration.ClientFactory(() => new AmazonS3Client(new EnvironmentVariablesAWSCredentials()
                    , new AmazonS3Config //for localstack
                    {
                        ServiceURL = "http://localhost:4572",
                        ForcePathStyle = true,
                        UseHttp = true,
                    }));

                endpointConfiguration.UsePersistence<InMemoryPersistence>();

                endpointConfiguration.EnableInstallers();

                var conventions = endpointConfiguration.Conventions();
                conventions.DefiningCommandsAs(
                    type =>
                    {
                        return type.Namespace.EndsWith("Messages.Commands");
                    });

                endpoint = await Endpoint.Start(endpointConfiguration);
            }
            catch (Exception ex)
            {
                FailFast("Failed to start.", ex);
            }
        }

        public async Task Stop()
        {
            try
            {
                // TODO: perform any futher shutdown operations before or after stopping the endpoint
                await endpoint?.Stop();
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }
        }

        async Task OnCriticalError(ICriticalErrorContext context)
        {
            // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
            // and consider setting up service recovery
            // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        void FailFast(string message, Exception exception)
        {
            try
            {
                log.Fatal(message, exception);

                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}
