namespace Blob.API.Configuration
{
    using System;

    public class S3Settings
    {
        public string Prefix { get; set; }
        public string StorageDomain { get; }
        public string BucketName { get; }
        public string ServiceUrl { get; set; }
        public bool ForcePathStyle { get; set; }

        public S3Settings(string bucketName, string storageDomain)
        {
            // todo storage domain can't start with a dot
            this.StorageDomain = string.IsNullOrEmpty(storageDomain)
                ? throw new ArgumentNullException($"{nameof(this.BucketName)} is missing")
                : storageDomain;

            this.BucketName = string.IsNullOrEmpty(bucketName)
                ? throw new ArgumentNullException($"{nameof(this.BucketName)} is missing")
                : bucketName;
        }
    }
}
