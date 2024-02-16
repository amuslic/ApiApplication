using Google.Protobuf;
using StackExchange.Redis;
using System.IO;
using System.Threading.Tasks;
using System;
using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Configuration;
using Microsoft.Extensions.Options;

namespace ApiApplication.Application
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly TimeSpan _cacheExpiration;

        public CacheService(IOptionsMonitor<RedisConfiguration> redisOptions)
        {
            var options = redisOptions.CurrentValue;
            _cacheExpiration = TimeSpan.FromMinutes(options.CacheExpirationTimeInMinutes);

            var redisConnection = ConnectionMultiplexer.Connect(options.ConnectionString);
            _database = redisConnection.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key) where T : IMessage<T>, new()
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }

            var message = new T();
            message.MergeFrom(value);
            return message;
        }

        public async Task SetAsync<T>(string key, T value) where T : IMessage<T>
        {
            using var stream = new MemoryStream();
            value.WriteTo(stream);
            await _database.StringSetAsync(key, stream.ToArray(), _cacheExpiration);
        }
    }
}
