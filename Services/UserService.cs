using Project_A_Server.Models;
using Project_A_Server.Utils;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.Redis;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<User> _repository;
        private readonly UserMeetingsService _userMeetings;
        private readonly UserAvailabilitiesService _userAvailabilities;
        private readonly UserGroupsService _userGroups;
        private readonly RedisService _cache;

        public UserService(
            IMongoClient mongoClient, IConfiguration configuration, IGenericRepository<User> repository,
            UserMeetingsService userMeetingsService, UserAvailabilitiesService userAvailabilities,
            UserGroupsService userGroups, RedisService projectARedisService)
        {
            var database = mongoClient.GetDatabase("ProjectA");

            _users = database.GetCollection<User>("Users");
            _repository = repository;
            _userMeetings = userMeetingsService;
            _userAvailabilities = userAvailabilities;
            _userGroups = userGroups;
            _configuration = configuration;
            _cache = projectARedisService;
        }

        // TODO: If user does not exist, throw an error

        public async Task<User> GetByUsernameAsync(string username)
        {
            var user = await _repository.GetByUsernameAsync(username);

            return user ?? throw new InvalidOperationException($"User with username '{username}' not found.");
        }

        public async Task CreateUserAsync(string username, string password)
        {
            var user = new User
            {
                UID = GuidGenerator.Generate(), 
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _repository.CreateAsync(user);

            var newUser = await _repository.GetByUIDAsync(user.UID);
            if (newUser?.Id is null) throw new InvalidOperationException("Failed to retrieve user ID after insertion.");

            var userMeetings = new UserMeetings
            {
                UID = user.UID,
                Meetings = []
            };

            var userAvailabilities = new UserAvailabilities
            {
                UID = user.UID,
                Availabilities = []
            };

            var userGroups = new UserGroups
            {
                UID = user.UID,
                Groups = []
            };

            await _userMeetings.CreateAsync(userMeetings);
            await _userAvailabilities.CreateAsync(userAvailabilities);
            await _userGroups.CreateAsync(userGroups);
            
            await _cache.CacheIDAsync(newUser.UID, newUser.Id);
        }

        public string GenerateJwtToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT secret key is not set in the environment variables.");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("uid", user.UID),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
