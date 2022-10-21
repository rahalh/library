namespace Media.API.Core
{
    using System.Threading.Tasks;

    public static class ProducedEventType
    {
        public static readonly string MediaRemoved = "MEDIA_REMOVED";
        public static readonly string BlobFailed = "BLOB_FAILED";
    }

    public static class ConsumedEventType
    {
        public static readonly string BlobUploaded = "MEDIA_BLOB_UPLOADED";
        public static readonly string BlobRemoved = "MEDIA_BLOB_REMOVED";
    }

    public interface IMediaEvent
    {
        public Task Removed(string id);
        public Task BlobFailed(string msg);
    }
}
