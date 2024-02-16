namespace ApiApplication.Application.Configuration
{
    public class RedisConfiguration
    {
        public static readonly string ConfigurationKey = "Redis";
        public string ConnectionString { get; set; }
        public int CacheExpirationTimeInMinutes { get; set; }
    }

}
