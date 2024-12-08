using MongoDB.Driver;

namespace Project_A_Server.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetByObjectIdAsync(string id);
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
        Task DeleteByObjectIdAsync(string id);

        IMongoCollection<T> GetCollection();
    }
}
