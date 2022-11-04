namespace Media.Infrastructure.Adapters
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Services;
    using Confluent.Kafka;

    public class KafkaEventProducer : IEventProducer
    {
        private readonly IProducer<Null, string> producer;

        public KafkaEventProducer(IProducer<Null, string> producer) => this.producer = producer;

        public async Task ProduceAsync<T>(Event<T> @event, CancellationToken token)
        {
            var topic = @event.EventType;
            var message = new Message<Null, string>() {Value = JsonSerializer.Serialize(@event)};
            await this.producer.ProduceAsync(topic, message, token);
        }
    }
}
