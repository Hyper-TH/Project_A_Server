using MongoDB.Driver;

namespace Project_A_Server.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByUsernameAsync(string username);
        Task<T?> GetByUIDAsync(string id);
        Task<T?> GetByMIDAsync(string id);
        Task<T?> GetByAIDAsync(string id);
        Task<T?> GetByGIDAsync(string id);
        Task<T?> GetByObjectIdAsync(string id);
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
        Task DeleteAsync(string id);
        Task DeleteByObjectIdAsync(string id);

        IMongoCollection<T> GetCollection();
    }
}
