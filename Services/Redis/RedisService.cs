using StackExchange.Redis;

namespace Project_A_Server.Services.Redis
{
    public class RedisService {

        private readonly IDatabase _redisDb;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        public async Task<string?> GetCachedDocIdAsync(string id)
        {
            try
            {
                var result = await _redisDb.StringGetAsync(id);
                return result.IsNullOrEmpty ? null : result.ToString();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error accessing cache for key {id}: {ex.Message}");
                return null;
            }
        }

        public async Task CacheIDAsync(string id, string docId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(docId))
                throw new ArgumentException("ID and docID must not be null or empty.");

            try
            {
                await _redisDb.StringSetAsync(id, docId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error caching ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveCachedIDAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty.", nameof(id));

            await _redisDb.KeyDeleteAsync(id);
        }
    }
}