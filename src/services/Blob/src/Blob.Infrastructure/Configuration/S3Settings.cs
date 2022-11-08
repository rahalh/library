namespace Blob.Infrastructure.Configuration
{
    using System.ComponentModel.DataAnnotations;

    public class S3Settings
    {
        [Required, MinLength(1), RegularExpression(@"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$", ErrorMessage = "Invalid domain name")]
        public string StorageDomain { get; set; }
        [Required, MinLength(1)]
        public string BucketName { get; set; }
        public string? Prefix { get; set; }
        public string? ServiceUrl { get; set; }
        public bool ForcePathStyle { get; set; }
    }
}
