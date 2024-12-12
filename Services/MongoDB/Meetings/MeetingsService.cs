using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Interfaces;

namespace Project_A_Server.Services.MongoDB.Meetings
{
    public class MeetingsService
    {
        private readonly IGenericRepository<Meeting> _repository;

        public MeetingsService(IGenericRepository<Meeting> repository)
        {
            _repository = repository;
        }

        public async Task<List<Meeting>> GetAllAsync() =>
            await _repository.GetAllAsync();

        public async Task<Meeting?> GetAsync(string id) =>
            await _repository.GetByObjectIdAsync(id);

        public async Task CreateAsync(Meeting newMeeting) =>
            await _repository.CreateAsync(newMeeting);

        public async Task UpdateAsync(string id, Meeting updatedMeeting) =>
            await _repository.UpdateAsync(id, updatedMeeting);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);
    }
}
