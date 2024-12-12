using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Models.Meetings;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class UserAvailabilitiesService
    {
        private readonly IGenericRepository<UserAvailabilities> _repository;

        public UserAvailabilitiesService(IGenericRepository<UserAvailabilities> repository)
        {
            _repository = repository;
        }

        public async Task<List<UserAvailabilities>> GetAllAsync() => 
            await _repository.GetAllAsync();

        public async Task<UserAvailabilities?> GetByUIDAsync(string uid) =>
            await _repository.GetByIdAsync(uid);

        public async Task CreateAsync(UserAvailabilities newAvailability) =>
            await _repository.CreateAsync(newAvailability);

        public async Task UpdateAsync(string id, UserAvailabilities updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);

        public async Task AddAvailabilityAsync(string uid, string aid)
        {
            var collection = _repository.GetCollection();
            var update = Builders<UserAvailabilities>.Update.Push(x => x.Availabilities, aid);
            var result = await collection.UpdateOneAsync(x => x.UID == uid, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"User with ID {uid} not found.");
        }

    }
}
