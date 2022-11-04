namespace Media.Application.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ProducedEvents
    {
        public const string MediaRemoved = "MEDIA_REMOVED";
        public const string MediaUpdateFailed = "MEDIA_UPDATE_FAILED";
    }

    public static class ConsumedEvents
    {
        public const string BlobUploaded = "BLOB_UPLOADED";
        public const string BlobRemoved = "BLOB_REMOVED";
    }

    public record Event<T> (
        DateTime DispatchTime,
        string EventType,
        string ServiceName,
        T Content
    );

    public interface IEventProducer
    {
        public Task ProduceAsync<T>(Event<T> @event, CancellationToken token);
    }
}
