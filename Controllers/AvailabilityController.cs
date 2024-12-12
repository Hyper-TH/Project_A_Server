using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.MongoDB.Meetings;
using Microsoft.AspNetCore.Http.Connections;
using System.Security.Cryptography;

namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilitiesService _availabilitiesService;
        private readonly UserAvailabilitiesService _userAvailabilitiesService;
        private readonly RedisService _cache;

        public AvailabilityController(AvailabilitiesService availabilities, UserAvailabilitiesService userAvailabilities, RedisService cache)
        {
            _availabilitiesService = availabilities;
            _userAvailabilitiesService = userAvailabilities;
            _cache = cache;
        }

        [HttpGet("availability/{aid}")]
        public async Task<IActionResult> GetAvailability(string aid)
        {
            if (string.IsNullOrEmpty(aid))
            {
                return BadRequest("Availability ID cannot be null or empty.");
            }

            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(aid);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {aid} does not exist." });
                }

                var meeting = await _availabilitiesService.GetAsync(cachedDocId);

                if (meeting == null)
                {
                    return NotFound($"Availability with ID '{aid}' was not found.");
                }

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Meeting: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the meeting." });
            }
        }

        [HttpGet("availabilities/{uID}")]
        public async Task<IActionResult> GetUserAvailabilities(string uID)
        {
            if (string.IsNullOrEmpty(uID))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                var userAvailabilities = await _userAvailabilitiesService.GetByUIDAsync(uID);

                if (userAvailabilities == null)
                {
                    return NotFound($"User with ID '{uID}' was not found.");
                }

                var availabilityDetails = new List<Availability>();

                foreach (var aid in userAvailabilities.Availabilities)
                {
                    try
                    {
                        var cachedDocId = await _cache.GetCachedDocIdAsync(aid);
                        if (string.IsNullOrEmpty(cachedDocId))
                        {
                            Console.WriteLine($"Availability ID '{aid}' does not exist in Redis.");
                            continue; // Skip this availability if no docID is found
                        }

                        var availability = await _availabilitiesService.GetAsync(cachedDocId);
                        if (availability != null)
                        {
                            availabilityDetails.Add(availability);
                        }
                        else
                        {
                            Console.WriteLine($"Availability with docID '{cachedDocId}' not found in MongoDB.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing Availability ID '{aid}': {ex.Message}\n{ex.StackTrace}");
                    }
                }

                return Ok(availabilityDetails);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving User's Availabilities: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the availabilities." });
            }
        }


        [HttpPost("availability")]
        public async Task<IActionResult> Post([FromBody] Availability newAvailability)
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

                    Console.WriteLine($"Collision detected for aID: {aID}. Generating a new one.");
                }
                while (true);

                newAvailability.aID = aID;

                await _availabilitiesService.CreateAsync(newAvailability);
                await _userAvailabilitiesService.AddAvailabilityAsync(newAvailability.uID, newAvailability.aID);
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
