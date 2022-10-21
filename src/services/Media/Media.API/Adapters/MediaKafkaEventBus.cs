namespace Media.API.Adapters
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Confluent.SchemaRegistry.Serdes;
    using Core;
    using Microsoft.Extensions.Configuration;
    using Serilog;

    public class MediaKafkaEvent : IMediaEvent
    {
        // todo use polly circuit breaker
        // todo why is circuit breaker not used with redis ?
        private readonly IProducer<Null, string> producer;
        private readonly ILogger logger;

        public MediaKafkaEvent(ILogger logger, IConfiguration config)
        {
            this.logger = logger.ForContext<MediaKafkaEvent>();
            var pconfig = new ProducerConfig()
            {
                BootstrapServers = config.GetConnectionString("Kafka")
            };
            this.producer = new ProducerBuilder<Null, string>(pconfig).Build();
        }

        public async Task Removed(string id)
        {
            // todo don't log anything here
            var topic = $"kafka://{ProducedEventType.MediaRemoved}";
            var message = new MediaEvent(
                new MediaEventMetadata(id, EventType.EventDomain),
                id
            );
            await this.producer.ProduceAsync(topic, new Message<Null, string>() { Value = JsonSerializer.Serialize(message) });
        }

        public async Task BlobFailed(string msg)
        {
            var topic = $"kafka://{ProducedEventType.BlobFailed}";
            var message = ""; // fixme
            await this.producer.ProduceAsync(topic, new Message<Null, string>() { Value = JsonSerializer.Serialize(message) });
        }
    }
}
