namespace Media.Infrastructure.Configuration
{
    using System.ComponentModel.DataAnnotations;

    public class RedisSettings
    {
        [Required, MinLength(1)]
        public string ConnectionString { get; set; }
    }
}
