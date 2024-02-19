namespace ApiApplication.Application.Configuration
{
    public record RedisConfiguration
    {
        public static readonly string ConfigurationKey = "Redis";
        public string ConnectionString { get; init; }
        public int CacheExpirationTimeInMinutes { get; init; }
    }

}
