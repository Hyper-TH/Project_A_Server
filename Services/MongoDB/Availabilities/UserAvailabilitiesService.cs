using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class UserAvailabilitiesService
    {
        private readonly IGenericRepository<UserAvailabilities> _repository;

        public UserAvailabilitiesService(IGenericRepository<UserAvailabilities> repository)
        {
            _repository = repository;
        }

        public async Task<UserAvailabilities?> GetAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentNullException(nameof(uid), "Invalid UID.");

            var availabilities = await _repository.GetByUIDAsync(uid);

            return availabilities ?? throw new KeyNotFoundException($"No Availabilities found for UID: {uid}.");
        }

        public async Task CreateAsync(UserAvailabilities newAvailability) =>
            await _repository.CreateAsync(newAvailability);

        public async Task UpdateAsync(string id, UserAvailabilities updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);

        public async Task AddAvailabilityAsync(string uid, string aid)
        {
            if (uid == null || aid == null)
            {
                var nullValue = uid == null ? nameof(uid) : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null.");
            }

            var filter = Builders<UserAvailabilities>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserAvailabilities>.Update.AddToSet(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"User with ID {uid} not found.");
        }

        public async Task RemoveAvailabilityAsync(string uid, string aid)
        {
            if (uid == null || aid == null)
            {
                var nullValue = uid == null ? nameof(uid) : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null.");
            }

            var filter = Builders<UserAvailabilities>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserAvailabilities>.Update.Pull(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"User with ID {uid} not found.");
        }
    }
}
