namespace Media.API.Ports.Events.Handlers
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core.Interactors;
    using Serilog;

    public record BlobUploadedEvent(string Id, string URL);

    public static  class BlobUploadedHandler
    {
        public static async Task HandleAsync(ILogger logger, SetContentURLInteractor handler, Null key, string value)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<BlobUploadedEvent>(value);
                await handler.Handle(new SetContentURLRequest(msg.Id, msg.URL), CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
    }
}
