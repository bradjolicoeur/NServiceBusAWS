using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Example.ProducerWorker.Jobs;
using FluentScheduler;
using Autofac;
using Example.NSBConfiguration;

namespace Example.ProducerWorker
{
    class Host
    {
        // TODO: optionally choose a custom logging library
        // https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        static readonly ILog log = LogManager.GetLogger<Host>();

        private static readonly string TransportConfiguration = Environment.GetEnvironmentVariable("TRANSPORT_CONFIGURATION");

        IEndpointInstance endpoint = null;

        // TODO: give the endpoint an appropriate name
        public string EndpointName => "example.producerworker";

        public async Task Start()
        {
            try
            {

                var builder = new ContainerBuilder();

                var endpointConfiguration = NSBEndpointConfiguration.ConfigureEndpoint(builder, EndpointName);

                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                endpoint = await Endpoint.Start(endpointConfiguration);

                //schedule send job
                JobManager.AddJob(
                new SendMessageJob(endpoint),
                schedule =>
                {
                    schedule
                        .ToRunNow()
                        .AndEvery(3).Seconds();
                });

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
                JobManager.StopAndBlock();

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
