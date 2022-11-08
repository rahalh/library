namespace Blob.Infrastructure.Configuration
{
    using System.ComponentModel.DataAnnotations;

    public class DDBSettings
    {
        [Required, MinLength(1)]
        public string TableName { get; set; }
        public string PartitionKey { get; set; }
        public string? ServiceURL { get; set; }
    }
}
