namespace Media.API.Adapters.Kafka.Producer
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;
    using Microsoft.Extensions.Configuration;

    public class MediaKafkaEventBus : IMediaEventBus
    {
        private readonly IProducer<Null, string> producer;

        public MediaKafkaEventBus(IConfiguration config)
        {
            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = config.GetConnectionString("Kafka")
            };
            this.producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task PublishAsync(string eventType, string eventContent)
        {
            var topic = eventType;
            var message = new MediaEvent(
                new MediaEventMetadata(EventType.EventDomain),
                eventContent
            );
            await this.producer.ProduceAsync(topic, new Message<Null, string>() { Value = JsonSerializer.Serialize(message) });
        }
    }
}
