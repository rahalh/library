namespace Media.API.Ports.Events.Handlers
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Core.Interactors;
    using Serilog;

    public record BlobRemovedEvent(string Id);

    public static class BlobRemovedHandler
    {
        // todo handleAsync
        public static async Task HandleAsync(ILogger logger, SetContentURLInteractor handler, Null key, string value)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<BlobRemovedEvent>(value);
                await handler.Handle(new SetContentURLRequest(msg.Id, null), CancellationToken.None);
            }
            catch (Exception ex)
            {
                // todo complete this
                logger.Error(ex, ex.Message);
            }
        }
    }
}
