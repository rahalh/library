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
        public string Id { get; }
        public string Name { get; }
        public long Size { get; }
        public string BlobType { get; }
        public string Extension { get; }
        public string URL { get; }
        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; }

        public Blob(string id, string blobType, string extension, long size, string storageDomain, string storagePath)
        {
            this.Id = id;
            this.Name = $"{id}.{extension}";
            this.Size = size;
            this.BlobType = blobType;
            this.Extension = Strings.LCase(extension);
            this.CreateTime = new DateTime();
            this.UpdateTime = new DateTime();

            this.URL = $"https://{storageDomain}/{storagePath}/media/{this.Name}";
        }
    }
}
