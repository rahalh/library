namespace Blob.Infrastructure.Transport.Events
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Application.Services;
    using Confluent.Kafka;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public class BackgroundKafkaConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IConfiguration config;
        private readonly ILogger logger;
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
        private readonly int delay = 5000;
        private readonly string[] topics = new[] {ConsumedEvents.MediaUpdateFailed};

        public BackgroundKafkaConsumer(ILogger logger, IServiceScopeFactory serviceScopeFactory, IConfiguration config)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.config = config;
            this.logger = logger.ForContext<BackgroundKafkaConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var builder = new ConsumerBuilder<Null, string>(new ConsumerConfig()
            {
                BootstrapServers = this.config.GetConnectionString("Kafka"),
                GroupId = "blob_event_consumers",
                AllowAutoCreateTopics = true
            });

            using var consumer = builder.Build();
            consumer.Subscribe(this.topics);

            while (!stoppingToken.IsCancellationRequested)
            {
                string message = null;
                try
                {
                    var result = consumer.Consume(this.timeout);
                    if (result != null)
                    {
                        if (result.Topic == ConsumedEvents.MediaUpdateFailed)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService<DeleteBlobSagaInteractor>();
                            message = result.Message.Value;
                            var request = JsonSerializer.Deserialize<DeleteBlobSagaRequest>(message);
                            await handler.HandleAsync(request, CancellationToken.None);
                        }

                        consumer.Commit(result);
                        consumer.StoreOffset(result);
                    }
                }
                catch (Exception ex)
                {
                    this.logger
                        .ForContext("Event", message)
                        .Error(ex, message is not null ? "Error while processing an event" : ex.Message);
                }
                finally
                {
                    await Task.Delay(this.delay, stoppingToken);
                }
            }
        }
    }
}
