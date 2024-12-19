using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class GroupsService
    {
        private readonly IGenericRepository<Group> _repository;

        public GroupsService(IGenericRepository<Group> repository)
        {
            _repository = repository;
        }

        public async Task<List<Group>> GetAllAsync() =>
           await _repository.GetAllAsync();

        public async Task<Group?> GetAsync(string id) =>
            await _repository.GetByGIDAsync(id);

        public async Task<Group?> GetByIdAsync(string id) =>
            await _repository.GetByObjectIdAsync(id);

        public async Task CreateAsync(Group newGroup) =>
            await _repository.CreateAsync(newGroup);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteAsync(id);

        public async Task AddUserToGroupAsync(string uid, string id)
        {
            var collection = _repository.GetCollection();
            var filter = Builders<Group>.Filter.Eq(x => x.Id, id);
            var update = Builders<Group>.Update.AddToSet(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Group with object ID {id} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is already in the Group object ID {id}.");
        }

        public async Task RemoveUserFromGroupAsync(string uid, string id)
        {
            var collection = _repository.GetCollection();
            var filter = Builders<Group>.Filter.Eq(x => x.Id, id);
            var update = Builders<Group>.Update.Pull(x => x.Users, uid);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Group with object ID {id} not found.");

            if (result.ModifiedCount == 0)
                Console.WriteLine($"User {uid} is not in the Group object ID {id}.");
        }

    }
}
