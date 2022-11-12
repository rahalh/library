namespace Media.Infrastructure.Transport.Events
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
        private readonly string[] topics = {ConsumedEvents.BlobRemoved, ConsumedEvents.BlobUploaded};

        // todo I think it is better to inject ConsumerConfig or even better the entire consumer
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
                GroupId = "media_event_consumers"
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
                        message = result.Message.Value;
                        if (result.Topic == ConsumedEvents.BlobUploaded)
                        {
                            var request = JsonSerializer.Deserialize<SetContentURLRequest>(message);
                            var handler = scope.ServiceProvider.GetRequiredService<SetContentURLInteractor>();
                            await handler.HandleAsync(request, CancellationToken.None);
                        }
                        else if (result.Topic == ConsumedEvents.BlobRemoved)
                        {
                            var request = JsonSerializer.Deserialize<RemoveContentURLRequest>(message);
                            var handler = scope.ServiceProvider.GetRequiredService<RemoveContentURLInteractor>();
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
