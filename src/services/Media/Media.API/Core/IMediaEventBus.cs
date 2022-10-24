namespace Media.API.Core
{
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

    public interface IMediaEventBus
    {
        public Task PublishAsync(string eventType, string eventContent);
    }
}
