namespace Blob.API.Config
{
    using System;

    public class S3Settings
    {
        public string Prefix { get; set; }
        public string BucketName { get; }
        public string ServiceUrl { get; set; }
        public bool ForcePathStyle { get; set; }

        public S3Settings(string bucketName) =>
            this.BucketName = string.IsNullOrEmpty(bucketName)
                ? throw new ArgumentNullException($"{nameof(BucketName)} is missing")
                : bucketName;
    }
}
