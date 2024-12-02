using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Project_A_Server.Models;
using Project_A_Server.Models.DatabaseSettings;

namespace Project_A_Server.Services.MongoDB
{
    public class MeetingsService
    {
        private readonly IMongoCollection<Meeting> _meetingsCollection;

        public MeetingsService(IOptions<DBSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

            _meetingsCollection = mongoDatabase.GetCollection<Meeting>(
                dbSettings.Value.MeetingsCollectionName);
        }

        public async Task<List<Meeting>> GetAsync() =>
            await _meetingsCollection.Find(_ => true).ToListAsync();

        public async Task<Meeting?> GetAsync(string mid) =>
            await _meetingsCollection.Find(x => x.mID == mid).FirstOrDefaultAsync();

        public async Task<Meeting?> GetByMIDAsync(string mid) =>
            await _meetingsCollection.Find(Meeting => Meeting.mID == mid).FirstOrDefaultAsync();

        public async Task CreateAsync(Meeting newMeeting) =>
            await _meetingsCollection.InsertOneAsync(newMeeting);

        public async Task UpdateAsync(string mid, Meeting updatedMeeting) =>
            await _meetingsCollection.ReplaceOneAsync(x => x.mID == mid, updatedMeeting);

        public async Task RemoveAsync(string mid) =>
            await _meetingsCollection.DeleteOneAsync(x => x.mID == mid);
    }
}
