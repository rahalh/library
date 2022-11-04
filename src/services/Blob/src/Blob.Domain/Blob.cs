namespace Blob.Domain
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
            this.BlobType = Strings.UCase(blobType);
            this.Extension = Strings.UCase(extension);
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;

            this.URL = new UriBuilder() {
                Host = storageDomain,
                Scheme = "https",
                Path = Strings.Join(new [] {prefix, this.Name}, "/")!
            }.Uri.ToString();
        }

        public Blob() { }
    }
}
