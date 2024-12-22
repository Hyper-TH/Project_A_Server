using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class GroupAvailabilitiesService
    {
        private readonly IGenericRepository<GroupAvailabilities> _repository;
        
        public GroupAvailabilitiesService(IGenericRepository<GroupAvailabilities> repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(GroupAvailabilities newGroupAvailabilities) =>
            await _repository.CreateAsync(newGroupAvailabilities);

        public async Task<GroupAvailabilities> GetAllAsync(string gid)
        {
            if (gid == null)
                throw new ArgumentNullException(nameof(gid), "Group ID can not be null.");
            
            var groupAvailabilities = await _repository.GetByGIDAsync(gid);

            return groupAvailabilities ?? throw new KeyNotFoundException($"No Availabilities found for GID: {gid}.");
        }

        public async Task AddAvailabilityAsync(string gid, string aid)
        {
            if (gid == null || aid == null)
            {
                var nullValue = gid == null ? nameof(gid) : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null.");
            }

            var filter = Builders<GroupAvailabilities>.Filter.Eq(x => x.gID, gid);
            var update = Builders<GroupAvailabilities>.Update.AddToSet(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Group with ID {gid} not found.");
        }

        public async Task RemoveAvailabilityAsync(string gid, string aid)
        {
            if (gid == null || aid == null)
            {
                var nullValue = gid == null ? nameof(gid) : nameof(aid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null.");

            }
            var filter = Builders<GroupAvailabilities>.Filter.Eq(x => x.gID, gid);
            var update = Builders<GroupAvailabilities>.Update.Pull(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Group with ID {gid} not found.");
        }
    }
}
