using Project_A_Server.Models;

namespace Project_A_Server.Services.Redis.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(string username, string password);
        string GenerateJwtToken(User user);
    }
}
