using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.DatabaseSettings;
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
            await _repository.GetByIdAsync(id);

        public async Task CreateAsync(Attendees newAttendee) =>
            await _repository.CreateAsync(newAttendee);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteAsync(id);

        public async Task AddUserToMeetingAsync(string uid, string mid)
        {
            var collection = _repository.GetCollection();
            var filter = Builders<Attendees>.Filter.Eq(x => x.Id, mid);
            var update = Builders<Attendees>.Update.AddToSet(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is already in the meeting {mid}.");
        }

        public async Task RemoveUserFromMeetingAsync(string uid, string mid)
        {
            var collection = _repository.GetCollection();
            var filter = Builders<Attendees>.Filter.Eq(x => x.Id, mid);
            var update = Builders<Attendees>.Update.Pull(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is not in the meeting {mid}.");
        }
    }

}
