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
            await _repository.GetByUIDAsync(uid);

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