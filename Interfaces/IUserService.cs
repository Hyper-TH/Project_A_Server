﻿using Project_A_Server.Models;

namespace Project_A_Server.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByUsernameAsync(string username);
        Task CreateUserAsync(string username, string password);
        string GenerateJwtToken(User user);
    }
}
