namespace Media.Domain
{
    using System;
    using Microsoft.VisualBasic;
    using Nanoid;

    public enum MediaType
    {
        Book,
        Video,
        Podcast
    }

    public class Media
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        // Using ISO 639-1 Language code
        public string LanguageCode { get; set; }
        public string MediaType { get; set; }
        public int TotalViews { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string ContentURL { get; set; }

        public Media(string title, string description, string languageCode, DateTime publishDate, string mediaType)
        {
            this.Title = title;
            this.Description = description;
            this.PublishDate = publishDate;
            this.MediaType = Strings.UCase(mediaType);
            this.ExternalId = Nanoid.Generate();
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;
            this.LanguageCode = languageCode ?? "en";
        }

        public Media() {}
    }
}
