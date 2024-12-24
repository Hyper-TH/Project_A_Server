using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class UserGroupsService
    {
        private readonly IGenericRepository<UserGroups> _repository;

        public UserGroupsService(IGenericRepository<UserGroups> repository)
        {
            _repository = repository;
        }
        public async Task CreateAsync(UserGroups newUserGroups) =>
            await _repository.CreateAsync(newUserGroups);

        public async Task<UserGroups> GetAllAsync(string uid)
        {
            if (uid == null)
                throw new ArgumentNullException(nameof(uid), "Group ID can not be null.");

            var groupAvailabilities = await _repository.GetByUIDAsync(uid);

            return groupAvailabilities ?? throw new KeyNotFoundException($"No Groups found for UID: {uid}.");
        }

        public async Task<UserGroups> AddGroupAsync(string uid, string gid)
        {

            var newGroup = new UserGroups.Group
            {
                gID = gid,
                Availabilities = []
            };

            var collection = _repository.GetCollection();
            var filter = Builders<UserGroups>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserGroups>.Update.AddToSet(x => x.Groups, newGroup);
            
            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"No user with UID {uid} found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"Group with gID {gid} already exists for user {uid}.");

            return await _repository.GetByUIDAsync(uid)
                   ?? throw new InvalidOperationException($"Failed to retrieve user groups for UID: {uid}.");
        }

        public async Task AddAvailabilityAsync(string uid, string gid, string aid)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(gid) || string.IsNullOrEmpty(aid))
            {
                var nullValue = string.IsNullOrEmpty(uid)
                    ? nameof(uid)
                    : string.IsNullOrEmpty(gid)
                        ? nameof(gid)
                        : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null or empty.");
            }

            var filter = Builders<UserGroups>.Filter.And(
                Builders<UserGroups>.Filter.Eq(x => x.UID, uid),
                Builders<UserGroups>.Filter.ElemMatch(x => x.Groups, g => g.gID == gid)
            );

            var update = Builders<UserGroups>.Update.Push("Groups.$.Availabilities", aid);

            var result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"No group with ID {gid} found for user with UID {uid}.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"Availability ID {aid} was already in the group with gID {gid} for user {uid}.");
        }
        public async Task RemoveAvailabilityAsync(string uid, string gid, string aid)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(gid) || string.IsNullOrEmpty(aid))
            {
                var nullValue = string.IsNullOrEmpty(uid)
                    ? nameof(uid)
                    : string.IsNullOrEmpty(gid)
                        ? nameof(gid)
                        : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null or empty.");
            }

            var filter = Builders<UserGroups>.Filter.And(
                Builders<UserGroups>.Filter.Eq(x => x.UID, uid),
                Builders<UserGroups>.Filter.ElemMatch(x => x.Groups, g => g.gID == gid)
            );

            var update = Builders<UserGroups>.Update.Pull("Groups.$.Availabilities", aid);

            var result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"No group with ID {gid} found for user with UID {uid}.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"Availability ID {aid} was not found in the group with gID {gid} for user {uid}.");
        }
    }
}
