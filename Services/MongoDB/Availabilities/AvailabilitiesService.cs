using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Services.Redis;
using Project_A_Server.Utils;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class AvailabilitiesService
    {
        private readonly IGenericRepository<Availability> _repository;
        private readonly RedisService _cache;

        public AvailabilitiesService(IGenericRepository<Availability> repository, RedisService redisService)
        {
            _repository = repository;
            _cache = redisService;
        }

        public async Task<List<Availability>> GetAllAsync() => 
            await _repository.GetAllAsync();

        public async Task<Availability?> GetAsync(string aid)
        {
            if (aid == null)
                throw new ArgumentNullException(nameof(aid), "Availability ID can not be null.");

            var cachedDocId = await _cache.GetCachedDocIdAsync(aid);

            if (string.IsNullOrEmpty(cachedDocId))
                throw new KeyNotFoundException($"Cached Doc ID for {aid} not found");

            return await _repository.GetByObjectIdAsync(cachedDocId);
        }

        public async Task<Availability> CreateAsync(Availability newAvailability)
        {
            if (newAvailability == null)
                throw new ArgumentNullException(nameof(newAvailability), "Invalid Availability data.");

            string aID;
            do
            {
                aID = GuidGenerator.Generate();
                var cachedDocId = await _cache.GetCachedDocIdAsync(aID);

                if (string.IsNullOrEmpty(cachedDocId))
                {
                    newAvailability.aID = aID;
                    break;
                }
            }
            while (true);

            await _repository.CreateAsync(newAvailability);

            var newData = await _repository.GetByAIDAsync(aID);

            if (newData?.Id == null)
                throw new InvalidOperationException("Failed to retrieve Object ID after insertion.");

            await _cache.CacheIDAsync(aID, newData.Id);

            return newData;
        }

        public async Task UpdateAsync(string id, Availability updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);
    }
}
