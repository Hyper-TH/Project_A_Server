using Project_A_Server.Models;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.Redis;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Interfaces;

namespace Project_A_Server.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly UserMeetingsService _userMeetings;
        private readonly IConfiguration _configuration;
        private readonly RedisService _cache;

        public UserService(IMongoClient mongoClient, IConfiguration configuration, UserMeetingsService userMeetingsService, RedisService projectARedisService)
        {
            var database = mongoClient.GetDatabase("ProjectA");
            _users = database.GetCollection<User>("Users");
            _userMeetings = userMeetingsService;
            _configuration = configuration;
            _cache = projectARedisService;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }
        public async Task<User?> GetUserByUidAsync(string uid)
        {
            return await _users.Find(u => u.UID == uid).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(string username, string password)
        {
            var user = new User
            {
                UID = Guid.NewGuid().ToString("N"),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _users.InsertOneAsync(user);
            await _cache.CacheIDAsync(user.UID, user.Id);

            var userMeetings = new UserMeetings
            {
                UID = user.UID,
                Meetings = Array.Empty<string>()
            };

            await _userMeetings.CreateAsync(userMeetings);
        }

        public string GenerateJwtToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY"); ;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("uid", user.UID),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }; ;

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
