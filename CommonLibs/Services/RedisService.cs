using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public interface IRedisService
    {
        Task<T?> GetAsync<T>(string key);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<long> IncrementAsync(string key, long value = 1);
        Task<bool> ExpireAsync(string key, TimeSpan expireTime);
    }

    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer connection)
        {
            _connection = connection;
            _database = _connection.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue) return default;

            return JsonSerializer.Deserialize<T>((string)value!);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var json = JsonSerializer.Serialize(value);
            return await _database.StringSetAsync(key, json, expiration);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        // --- Các hàm nâng cao ---

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await _database.StringIncrementAsync(key, value);
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expireTime)
        {
            return await _database.KeyExpireAsync(key, expireTime);
        }
    }
}
