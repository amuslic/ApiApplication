using Google.Protobuf;
using StackExchange.Redis;
using System.IO;
using System.Threading.Tasks;
using System;
using ApiApplication.Application.Abstractions;

namespace ApiApplication.Application
{
    public class CacheService : ICacheService
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
        private readonly IDatabase _database;

        //add it from config
        public CacheService()
        {
            _redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
            _database = _redisConnection.GetDatabase();
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
