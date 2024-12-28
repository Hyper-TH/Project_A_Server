using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Services.Redis;
using Project_A_Server.Utils;
using StackExchange.Redis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class GroupsService
    {
        private readonly IGenericRepository<Group> _repository;
        private readonly RedisService _cache;

        public GroupsService(IGenericRepository<Group> repository, RedisService redisService)
        {
            _repository = repository;
            _cache = redisService;
        }

        public async Task<Group?> GetAsync(string gid)
        {
            if (gid == null)
                throw new ArgumentNullException(nameof(gid), "Group ID can not be null.");

            var cachedDocId = await _cache.GetCachedDocIdAsync(gid);

            if (string.IsNullOrEmpty(cachedDocId))
                throw new KeyNotFoundException($"Cached Doc ID for {gid} not found");

            return await _repository.GetByObjectIdAsync(cachedDocId);
        }

        public async Task<Group> CreateAsync(Group newGroup)
        {
            if (newGroup == null)
                throw new ArgumentNullException(nameof(newGroup), "Invalid group data.");

            string gID;
            do
            {
                gID = GuidGenerator.Generate();
                var cachedDocId = await _cache.GetCachedDocIdAsync(gID);

                if (string.IsNullOrEmpty(cachedDocId))
                {
                    newGroup.gID = gID;
                    break;
                }

                Console.WriteLine($"Collision detected for gID: {gID}. Generating a new one.");
            }
            while (true);

            await _repository.CreateAsync(newGroup);

            var newData = await _repository.GetByGIDAsync(gID);

            if (newData?.Id == null)
                throw new InvalidOperationException("Failed to retrieve Object ID after insertion.");

            await _cache.CacheIDAsync(gID, newData.Id);

            return newData;
        }

        public async Task AddUserToGroupAsync(string uid, string gid)
        {
            if (gid == null)
                throw new ArgumentNullException(nameof(gid), "Group ID can not be null.");

            var cachedDocId = await _cache.GetCachedDocIdAsync(gid);

            var collection = _repository.GetCollection();
            var filter = Builders<Group>.Filter.Eq(x => x.Id, cachedDocId);
            var update = Builders<Group>.Update.AddToSet(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Group with object ID {cachedDocId} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is already in the Group object ID {cachedDocId}.");
        }

        public async Task RemoveUserFromGroupAsync(string uid, string gid)
        {
            if (gid == null)
                throw new ArgumentNullException(nameof(gid), "Group ID can not be null.");

            var cachedDocId = await _cache.GetCachedDocIdAsync(gid);

            var collection = _repository.GetCollection();
            var filter = Builders<Group>.Filter.Eq(x => x.Id, cachedDocId);
            var update = Builders<Group>.Update.Pull(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Group with object ID {cachedDocId} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is not in the Group object ID {cachedDocId}.");
        }

    }
}
