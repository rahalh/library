namespace Media.API.Ports.Events.Handlers
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;

    public record BlobRemovedEvent(string Id);

    public static class BlobRemovedHandler
    {
        public static async Task HandleAsync(IMediaService srv, Null key, string value)
        {
            var msg = JsonSerializer.Deserialize<BlobRemovedEvent>(value);
            await srv.SetContentURL(msg.Id, null, CancellationToken.None);
        }
    }
}
