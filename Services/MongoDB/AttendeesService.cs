using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Project_A_Server.Models;
using Project_A_Server.Models.DatabaseSettings;

namespace Project_A_Server.Services.MongoDB
{
    public class AttendeesService
    {
        private readonly IMongoCollection<Attendees> _attendeesCollection;

        public AttendeesService(IOptions<DBSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

            _attendeesCollection = mongoDatabase.GetCollection<Attendees>(
                dbSettings.Value.AttendeesCollectionName);
        }

        public async Task<List<Attendees>> GetAsync() => 
            await _attendeesCollection.Find(_ => true).ToListAsync();

        public async Task<Attendees?> GetAsync(string mid) => 
            await _attendeesCollection.Find(x => x.Id == mid).FirstOrDefaultAsync();

        public async Task CreateAsync(Attendees newAttendees) => 
            await _attendeesCollection.InsertOneAsync(newAttendees);

        public async Task RemoveAsync(string mid) => 
            await _attendeesCollection.DeleteOneAsync(x => x.Id == mid);

        public async Task AddUserToMeetingAsync(string uid, string mid)
        {
            var filter = Builders<Attendees>.Filter.Eq(x => x.Id, mid);
            var update = Builders<Attendees>.Update.AddToSet(x => x.Users, uid);

            var result = await _attendeesCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");
            }

            if (result.ModifiedCount == 0)
            {
                Console.WriteLine($"User {uid} us already in the meeting {mid}");
            }
        }

        public async Task RemoveOneAsync(string uid, string mid)
        {
            var filter = Builders<Attendees>.Filter.Eq(x => x.Id, mid);
            var update = Builders<Attendees>.Update.Pull(x => x.Users, uid);

            var result = await _attendeesCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException($"Meeting with ID {mid} not found.");
            }

            if (result.ModifiedCount == 0)
            {
                Console.WriteLine($"User {uid} not found in meeting {mid}.");
            }
        }
    }
}
