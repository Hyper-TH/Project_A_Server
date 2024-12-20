using Project_A_Server.Models.Meetings;
using Project_A_Server.Interfaces;
using Project_A_Server.Services.Redis;
using Project_A_Server.Utils;
using System.Security.Cryptography;

namespace Project_A_Server.Services.MongoDB.Meetings
{
    public class MeetingsService
    {
        private readonly IGenericRepository<Meeting> _repository;
        private readonly RedisService _cache;

        public MeetingsService(IGenericRepository<Meeting> repository, RedisService redisService)
        {
            _repository = repository;
            _cache = redisService;
        }

        public async Task<List<Meeting>> GetAllAsync() =>
            await _repository.GetAllAsync();

        public async Task<Meeting?> GetAsync(string mid)
        {
            if (mid == null)
                throw new ArgumentNullException(nameof(mid), "Meeting ID can not be null.");

            var cachedDocId = await _cache.GetCachedDocIdAsync(mid);

            if (string.IsNullOrEmpty(cachedDocId))
                throw new KeyNotFoundException($"Cached Doc ID for {mid} not found");

            return await _repository.GetByObjectIdAsync(cachedDocId);
        }

        public async Task<Meeting?> GetByIdAsync(string id) =>
            await _repository.GetByObjectIdAsync(id);

        public async Task<Meeting> CreateAsync(Meeting newMeeting)
        {
            if (newMeeting == null) 
                throw new ArgumentNullException(nameof(newMeeting), "Invalid Meeting data.");

            string mID;
            do
            {
                mID = GuidGenerator.Generate();
                var cachedDocId = await _cache.GetCachedDocIdAsync(mID);

                if (string.IsNullOrEmpty(cachedDocId))
                {
                    newMeeting.mID = mID;
                    break;
                }
            }
            while (true);

            await _repository.CreateAsync(newMeeting);

            var newData = await _repository.GetByMIDAsync(mID);

            if (newData?.Id == null)
                throw new InvalidOperationException("Failed to retrieve Object ID after insertion.");

            await _cache.CacheIDAsync(mID, newData.Id);

            return newData;
        }

        public async Task UpdateAsync(string id, Meeting updatedMeeting) =>
            await _repository.UpdateAsync(id, updatedMeeting);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);
    }
}
