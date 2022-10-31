namespace Blob.API.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ProducedEvents
    {
        public static readonly string BlobUploaded = "BLOB_UPLOADED";
        public static readonly string BlobRemoved = "BLOB_REMOVED";
    }

    public static class ConsumedEvents
    {
        public static readonly string MediaUpdateFailed = "MEDIA_UPDATE_FAILED";
    }

    public record Event(
        DateTime DispatchTime,
        string EventType,
        object Content
    );

    public interface IEventProducer
    {
        public Task ProduceAsync(Event @event, CancellationToken token);
    }
}
