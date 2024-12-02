using Project_A_Server.Services.Redis.Interfaces;
using Project_A_Server.Models;
using StackExchange.Redis;

namespace Project_A_Server.Services.Redis
{
    public class SessionService : ISessionService
    {
        private readonly IConnectionMultiplexer _redis;

        public SessionService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SetSessionAsync(string token, User user)
        {
            var db = _redis.GetDatabase();
            var userJson = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            await db.StringSetAsync(token, userJson, TimeSpan.FromMinutes(30));
        }
        public async Task<User> GetSessionAsync(string token)
        {
            var db = _redis.GetDatabase();
            var userJson = await db.StringGetAsync(token);

            return userJson.IsNullOrEmpty ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<User>(userJson);
        }

        public async Task RemoveSessionAsync(string token)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(token);
        }
    }
}
