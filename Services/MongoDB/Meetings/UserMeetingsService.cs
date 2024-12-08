using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Meetings;

namespace Project_A_Server.Services.MongoDB.Meetings
{
    public class UserMeetingsService
    {
        private readonly IGenericRepository<UserMeetings> _repository;

        public UserMeetingsService(IGenericRepository<UserMeetings> repository)
        {
            _repository = repository;
        }

        public async Task<List<UserMeetings>> GetAllAsync() =>
            await _repository.GetAllAsync();

        public async Task<UserMeetings?> GetByUIDAsync(string uid) =>
            await _repository.GetByIdAsync(uid);

        public async Task CreateAsync(UserMeetings newUserMeeting) =>
            await _repository.CreateAsync(newUserMeeting);

        public async Task UpdateAsync(string uid, UserMeetings updatedMeeting) =>
            await _repository.UpdateAsync(uid, updatedMeeting);

        public async Task RemoveAsync(string uid) =>
            await _repository.DeleteAsync(uid);

        public async Task AddMeetingAsync(string uid, string mid)
        {
            var collection = _repository.GetCollection();
            var update = Builders<UserMeetings>.Update.Push(x => x.Meetings, mid);
            var result = await collection.UpdateOneAsync(x => x.UID == uid, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"User with ID {uid} not found.");
        }

        public async Task RemoveMeetingAsync(string uid, string mid)
        {
            var collection = _repository.GetCollection();
            var update = Builders<UserMeetings>.Update.Pull(x => x.Meetings, mid);
            var result = await collection.UpdateOneAsync(x => x.UID == uid, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"User with ID {uid} not found.");
        }
    }

}
