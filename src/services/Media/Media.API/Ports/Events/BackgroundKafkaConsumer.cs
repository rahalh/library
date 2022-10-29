namespace Media.API.Ports.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;
    using Core.Interactors;
    using Handlers;
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

        // todo I think it is better to inject ConsumerConfig or even better the entire consumer
        public BackgroundKafkaConsumer(ILogger logger, IServiceScopeFactory serviceScopeFactory, IConfiguration config)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.config = config;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = this.serviceScopeFactory.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<SetContentURLInteractor>();
            var builder = new ConsumerBuilder<Null, string>(new ConsumerConfig()
            {
                BootstrapServers = this.config.GetConnectionString("Kafka"),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                GroupId = "blob_event_consumer"
            });

            using var consumer = builder.Build();
            consumer.Subscribe(new [] {ConsumedEventType.BlobRemoved, ConsumedEventType.BlobUploaded});

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(this.timeout);
                if (result != null)
                {
                    try
                    {
                        switch (result.Topic)
                        {
                            case ConsumedEventType.BlobUploaded:
                                await BlobUploadedHandler.HandleAsync(this.logger, handler, null, result.Message.Value);
                                break;
                            case ConsumedEventType.BlobRemoved:
                                await BlobRemovedHandler.HandleAsync(this.logger, handler, null, result.Message.Value);
                                break;
                        }

                        consumer.Commit(result);
                        consumer.StoreOffset(result);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                    }
                }
            }
        }
    }
}
