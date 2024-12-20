using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.Redis;
using System.Security.Cryptography;

namespace Project_A_Server.Services.MongoDB.Meetings
{
    public class UserMeetingsService
    {
        private readonly IGenericRepository<UserMeetings> _repository;
        private readonly RedisService _cache;

        public UserMeetingsService(IGenericRepository<UserMeetings> repository, RedisService redisService)
        {
            _repository = repository;
            _cache = redisService;
        }

        public async Task<UserMeetings?> GetAsync(string uid)
        {
            if (uid == null)
                throw new ArgumentNullException(nameof(uid), "User ID can not be null.");

            var meetings = await _repository.GetByUIDAsync(uid);
            return meetings ?? throw new KeyNotFoundException($"No Meetings found for UID: {uid}.");
        }

        public async Task CreateAsync(UserMeetings newUserMeeting) =>
            await _repository.CreateAsync(newUserMeeting);

        public async Task AddMeetingAsync(string uid, string mid)
        {
            var filter = Builders<UserMeetings>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserMeetings>.Update.Push(x => x.Meetings, mid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Error adding meeting {mid} for {uid}.");
        }

        public async Task RemoveMeetingAsync(string uid, string mid)
        {
            var filter = Builders<UserMeetings>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserMeetings>.Update.Pull(x => x.Meetings, mid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Error adding removing {mid} for {uid}.");
        }
    }
}