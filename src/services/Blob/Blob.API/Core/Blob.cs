namespace Blob.API.Core
{
    using System;
    using Microsoft.VisualBasic;

    public enum BlobTypes
    {
        Application,
        Audio,
        Video
    }

    public class Blob
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string BlobType { get; set; }
        public string Extension { get; set; }
        public string URL { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public Blob(string id, string blobType, string extension, long size, string storageDomain, string prefix)
        {
            this.Id = id;
            this.Name = $"{id}.{extension}";
            this.Size = size;
            this.BlobType = blobType;
            this.Extension = Strings.LCase(extension);
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;

            this.URL = $"https://{storageDomain}/{prefix}/media/{this.Name}";
        }

        public Blob() { }
    }
}
