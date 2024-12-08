using Project_A_Server.Models;

namespace Project_A_Server.Interfaces
{
    public interface ISessionService
    {
        Task SetSessionAsync(string token, User user);
        Task<User> GetSessionAsync(string token);
        Task RemoveSessionAsync(string token);
    }
}
