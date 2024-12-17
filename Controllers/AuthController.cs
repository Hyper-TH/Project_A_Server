using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Auth;

namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;

        public AuthController(IUserService userService, ISessionService sessionService)
        {
            _userService = userService;
            _sessionService = sessionService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            await _userService.GetUserByUsernameAsync(registerDto.Username);

            await _userService.CreateUserAsync(registerDto.Username, registerDto.Password);
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);
            var token = _userService.GenerateJwtToken(user);

            await _sessionService.SetSessionAsync(token, user);

            return Ok(new
            {
                Token = token,
                UID = user.UID,
                Username = user.Username,
                Location = user.Location,
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _sessionService.RemoveSessionAsync(token);

            return Ok("Logged out successfully.");
        }
    }
}
