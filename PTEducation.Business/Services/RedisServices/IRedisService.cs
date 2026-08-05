using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.RedisServices
{
    public interface IRedisService
    {
        Task<string?> GetAsync(string key);
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        
        // For Message Queue (Write-Behind)
        Task<long> PushToQueueAsync(string queueName, string value);
        Task<List<string>> PopFromQueueBatchAsync(string queueName, int batchSize = 50);
        Task<long> GetQueueLengthAsync(string queueName);

        // For Chat History Caching
        Task<List<string>> GetListAsync(string key, int start = 0, int stop = -1);
        Task<long> ListLeftPushAsync(string key, string value);
        Task ListTrimAsync(string key, int start, int stop);
    }
}
