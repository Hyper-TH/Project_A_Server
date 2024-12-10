using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly UserAvailabilitiesService _availabilitiesService;
        private readonly RedisService _cache;

        public AvailabilityController(UserAvailabilitiesService availabilities, RedisService cache)
        {
            _availabilitiesService = availabilities;
            _cache = cache;
        }

        [HttpGet("availability/{id}")]
        public async Task<IActionResult> GetAvailability(string aID)
        {
            if (string.IsNullOrEmpty(aID))
            {
                return BadRequest("Availability ID cannot be null or empty.");
            }

            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(aID);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {aID} does not exist." });
                }

                var availability = await _availabilitiesService.GetAsync(cachedDocId);

                if (availability == null)
                {
                    return NotFound($"Meeting with ID '{aID}' was not found.");
                }

                return Ok(availability);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Availability: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the meeting." });
            }
        }

        [HttpPost("availability")]
        public async Task<IActionResult> Post([FromBody] UserAvailabilities newAvailability)
        {
            if (newAvailability == null)
            {
                return BadRequest("Invalid availability data");
            }

            try
            {
                string aID;
                do
                {
                    aID = Guid.NewGuid().ToString("N");
                    var cachedDocId = await _cache.GetCachedDocIdAsync(aID);

                    if (string.IsNullOrEmpty(cachedDocId))
                    {
                        newAvailability.aID = aID;
                        break;
                    }

                    Console.WriteLine($"Collision detected for mID: {aID}. Generating a new one.");
                }
                while (true);

                newAvailability.aID = aID;

                await _availabilitiesService.CreateAsync(newAvailability);
                await _cache.CacheIDAsync(newAvailability.aID, newAvailability.Id);

                return CreatedAtAction(nameof(GetAvailability), new { aID = newAvailability.aID }, newAvailability);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Availability: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while creating the meeting." });
            }
        }
    }
}
