using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class AvailabilitiesService
    {
        private readonly IGenericRepository<Availability> _repository;

        public AvailabilitiesService(IGenericRepository<Availability> repository)
        {
            _repository = repository;
        }

        public async Task<List<Availability>> GetAllAsync() => 
            await _repository.GetAllAsync();

        public async Task<Availability?> GetByIdAsync(string id) => 
            await _repository.GetByObjectIdAsync(id);

        public async Task CreateAsync(Availability newAvailability) =>
            await _repository.CreateAsync(newAvailability);

        public async Task UpdateAsync(string id, Availability updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);
    }
}
