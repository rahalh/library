namespace Media.API.Core
{
    using System;
    using Nanoid;

    public static class Status
    {
        public static readonly string StatusPending = "STATUS_PENDING";
        public static readonly string StatusDone = "STATUS_DONE";
    }

    public enum MediaType
    {
        Book,
        Video,
        Podcast
    }

    public class Media
    {
        public int Id { get; }
        public string ExternalId { get; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public string LanguageCode { get; set; } = "en";
        public string MediaType { get; set; }
        public int TotalViews { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string ContentURL { get; set; }
        public string Status { get; set; }

        public Media(string title, string description, DateTime publishDate, string mediaType)
        {
            this.Title = title;
            this.Description = description;
            this.PublishDate = publishDate;
            this.MediaType = mediaType;
            this.ExternalId = Nanoid.Generate();
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;
        }
    }
}
