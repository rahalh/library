namespace Media.API.Ports.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;
    using Handlers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public class BackgroundKafkaConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IConfiguration config;

        public BackgroundKafkaConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration config)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = this.serviceScopeFactory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IMediaService>();
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
                var result = consumer.Consume(TimeSpan.FromMilliseconds(1000));
                if (result != null)
                {
                    try
                    {
                        switch (result.Topic)
                        {
                            case ConsumedEventType.BlobUploaded:
                                await BlobUploadedHandler.HandleAsync(service, null, result.Message.Value);
                                break;
                            case ConsumedEventType.BlobRemoved:
                                await BlobRemovedHandler.HandleAsync(service, null, result.Message.Value);
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
