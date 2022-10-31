namespace Media.API.Core
{
    using System;
    using System.Threading.Tasks;

    public static class ProducedEventType
    {
        public const string MediaRemoved = "MEDIA_REMOVED";
        public const string BlobFailed = "BLOB_FAILED";
    }

    public static class ConsumedEventType
    {
        public const string BlobUploaded = "MEDIA_BLOB_UPLOADED";
        public const string BlobRemoved = "MEDIA_BLOB_REMOVED";
    }

    public record Event(
        DateTime DispatchTime,
        string EventType,
        string ServiceName,
        object Content
    );

    public interface IEventProducer
    {
        public Task ProduceAsync(Event @event);
    }
}
