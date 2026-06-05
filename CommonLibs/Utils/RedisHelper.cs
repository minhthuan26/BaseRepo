using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public static class RedisHelper
    {
        private static ConnectionMultiplexer? _connection;
        private static IDatabase? _database;
        private static string _instancePrefix = "BaseRepo_";

        /// <summary>
        /// Khởi tạo kết nối Redis cho Helper (Được gọi 1 lần duy nhất khi hệ thống Start)
        /// </summary>
        public static void Initialize(string connectionString, string instancePrefix = "BaseRepo_")
        {
            if (_connection == null || !_connection.IsConnected)
            {
                _connection = ConnectionMultiplexer.Connect(connectionString);
                _database = _connection.GetDatabase();
                _instancePrefix = instancePrefix;
            }
        }

        private static string KeyWithPrefix(string key) => $"{_instancePrefix}{key}";

        #region Các hàm đồng bộ (Sync) - Chạy cực nhanh, không cần await

        public static bool Set<T>(string key, T value, TimeSpan? cacheTime = null)
        {
            if (_database == null) return false;
            var json = JsonSerializer.Serialize(value);
            return _database.StringSet(KeyWithPrefix(key), json, cacheTime);
        }

        public static T? Get<T>(string key)
        {
            if (_database == null) return default;
            var value = _database.StringGet(KeyWithPrefix(key));
            if (!value.HasValue) return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public static bool Remove(string key)
        {
            if (_database == null) return false;
            return _database.KeyDelete(KeyWithPrefix(key));
        }

        public static bool Exists(string key)
        {
            if (_database == null) return false;
            return _database.KeyExists(KeyWithPrefix(key));
        }

        #endregion

        #region Các hàm nâng cao đặc thù (Chỉ RedisHelper mới làm được)

        /// <summary>
        /// Tự động tăng số (Increment) - Rất tốt để làm bộ đếm số thứ tự, mã tự sinh, giới hạn Request (Rate Limit)
        /// </summary>
        public static long Increment(string key, long value = 1)
        {
            if (_database == null) return 0;
            return _database.StringIncrement(KeyWithPrefix(key), value);
        }

        /// <summary>
        /// Đặt thời gian hết hạn mới cho một Key đã tồn tại
        /// </summary>
        public static bool Expire(string key, TimeSpan expireTime)
        {
            if (_database == null) return false;
            return _database.KeyExpire(KeyWithPrefix(key), expireTime);
        }

        #endregion
    }
}
