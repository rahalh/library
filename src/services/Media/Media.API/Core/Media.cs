namespace Media.API.Core
{
    using System;
    using System.Text.Json.Serialization;

    public static class Status
    {
        public static readonly string StatusPending = "STATUS_PENDING";
        public static readonly string StatusDone = "STATUS_DONE";
    }

    public static class MediaType
    {
        public static readonly string MediaBook = "MEDIA_BOOK";
        public static readonly string MediaVideo = "MEDIA_VIDEO";
        public static readonly string MediaPodcast = "MEDIA_PODCAST";
    }

    public class Media
    {
        [JsonIgnore]
        public int ID { get; set; } = 0;
        public string ExternalID { get; set; } = Nanoid.Nanoid.Generate();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public string LanguageCode { get; set; } = "en";
        public string MediaType { get; set; } = global::Media.API.Core.MediaType.MediaBook;
        public int TotalViews { get; set; } = 0;
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = global::Media.API.Core.Status.StatusPending;
    }
}
