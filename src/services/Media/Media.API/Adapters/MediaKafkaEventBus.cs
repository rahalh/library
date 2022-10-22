namespace Media.API.Adapters
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;
    using Microsoft.Extensions.Configuration;

    public class MediaKafkaEventBus : IMediaEventBus
    {
        // todo use polly circuit breaker
        private readonly IProducer<Null, string> producer;

        public MediaKafkaEventBus(IConfiguration config)
        {
            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = config.GetConnectionString("Kafka")
            };
            this.producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task Removed(string id)
        {
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
