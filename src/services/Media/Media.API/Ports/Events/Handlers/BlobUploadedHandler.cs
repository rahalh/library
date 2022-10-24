namespace Media.API.Ports.Events.Handlers
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core;

    public record BlobUploadedEvent(string Id, string URL);

    public static  class BlobUploadedHandler
    {
        public static async Task HandleAsync(IMediaService srv, Null key, string value)
        {
            var msg = JsonSerializer.Deserialize<BlobUploadedEvent>(value);
            if (msg.Id is null || msg.URL is null)
            {
                // tod fix me
                throw new ArgumentException("Id or URL are missing from the event");
            }
            await srv.SetContentURL(msg.Id, msg.URL, CancellationToken.None);
        }
    }
}
