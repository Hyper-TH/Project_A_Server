using MongoDB.Bson;
using MongoDB.Driver;
using Project_A_Server.Interfaces;

namespace Project_A_Server.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public GenericRepository(IMongoCollection<T> collection)
        {
            _collection = collection;
        }

        public async Task<List<T>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        // Regular IDs
        public async Task<T?> GetByUsernameAsync(string username) =>
            await _collection.Find(Builders<T>.Filter.Eq("Username", username)).FirstOrDefaultAsync();
        public async Task<T?> GetByUIDAsync(string id) =>
            await _collection.Find(Builders<T>.Filter.Eq("UID", id)).FirstOrDefaultAsync();
        public async Task<T?> GetByMIDAsync(string id) => 
            await _collection.Find(Builders<T>.Filter.Eq("mID", id)).FirstOrDefaultAsync();
        public async Task<T?> GetByAIDAsync(string id) =>
            await _collection.Find(Builders<T>.Filter.Eq("aID", id)).FirstOrDefaultAsync();
        public async Task<T?> GetByGIDAsync(string id) =>
            await _collection.Find(Builders<T>.Filter.Eq("gID", id)).FirstOrDefaultAsync();

        // Object IDs
        public async Task<T?> GetByObjectIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException($"Invalid ObjectId: {id}");
            }

            return await _collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity) =>
            await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(string id, T entity) =>
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);

        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update) =>
            await _collection.UpdateOneAsync(filter, update);

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));

        public async Task DeleteByObjectIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException($"Invalid ObjectId: {id}");
            }

            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", objectId));
        }
        
        public IMongoCollection<T> GetCollection() => _collection;
    }
}
