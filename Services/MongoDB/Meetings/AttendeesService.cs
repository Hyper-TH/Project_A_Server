using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Meetings;

namespace Project_A_Server.Services.MongoDB.Meetings
{
    public class AttendeesService
    {
        private readonly IGenericRepository<Attendees> _repository;

        public AttendeesService(IGenericRepository<Attendees> repository)
        {
            _repository = repository;
        }

        public async Task<List<Attendees>> GetAllAsync() =>
            await _repository.GetAllAsync();

        public async Task<Attendees?> GetAsync(string id) =>
            await _repository.GetByMIDAsync(id);

        public async Task CreateAsync(string mid)
        {
            if (string.IsNullOrEmpty(mid))
                throw new ArgumentNullException(nameof(mid), "Meeting ID cannot be null or empty.");
            
            var newData = new Attendees
            {
                mID = mid,
                Users = []
            };

            await _repository.CreateAsync(newData);
        }

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteAsync(id);

        public async Task AddUserToMeetingAsync(string uid, string mid)
        {
            var filter = Builders<Attendees>.Filter.Eq(x => x.mID, mid);
            var update = Builders<Attendees>.Update.AddToSet(x => x.Users, uid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is already in the meeting {mid}.");
        }

        public async Task RemoveUserFromMeetingAsync(string uid, string mid)
        {
            var filter = Builders<Attendees>.Filter.Eq(x => x.mID, mid);
            var update = Builders<Attendees>.Update.Pull(x => x.Users, uid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is not in the meeting {mid}.");
        }
    }
}