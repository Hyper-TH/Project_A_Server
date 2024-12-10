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

        public async Task<List<UserAvailabilities>> GetAllAsync() => 
            await _repository.GetAllAsync();

        public async Task<UserAvailabilities?> GetAsync(string id) =>
            await _repository.GetByObjectIdAsync(id);

        public async Task CreateAsync(UserAvailabilities newAvailability) =>
            await _repository.CreateAsync(newAvailability);

        public async Task UpdateAsync(string id, UserAvailabilities updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);
    }
}
