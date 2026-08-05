using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace PTEducation.Business.Services.RedisServices
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (expiry.HasValue)
            {
                return await _db.StringSetAsync(key, value, expiry.Value);
            }
            return await _db.StringSetAsync(key, value);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }

        public async Task<long> PushToQueueAsync(string queueName, string value)
        {
            return await _db.ListRightPushAsync(queueName, value);
        }

        public async Task<List<string>> PopFromQueueBatchAsync(string queueName, int batchSize = 50)
        {
            var items = new List<string>();
            for (int i = 0; i < batchSize; i++)
            {
                var value = await _db.ListLeftPopAsync(queueName);
                if (!value.HasValue) break;
                
                items.Add(value.ToString());
            }
            return items;
        }

        public async Task<long> GetQueueLengthAsync(string queueName)
        {
            return await _db.ListLengthAsync(queueName);
        }

        public async Task<List<string>> GetListAsync(string key, int start = 0, int stop = -1)
        {
            var values = await _db.ListRangeAsync(key, start, stop);
            var list = new List<string>();
            foreach (var val in values)
            {
                if (val.HasValue) list.Add(val.ToString());
            }
            return list;
        }

        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            return await _db.ListLeftPushAsync(key, value);
        }

        public async Task ListTrimAsync(string key, int start, int stop)
        {
            await _db.ListTrimAsync(key, start, stop);
        }
    }
}
