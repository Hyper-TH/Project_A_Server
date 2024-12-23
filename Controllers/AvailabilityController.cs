using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Models.Availabilities;
using Microsoft.AspNetCore.Authorization;

// TODO: Reduce Boilerplating, collections that store id: [{}] have similar logic 
namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilitiesService _availabilitiesService;
        private readonly UserAvailabilitiesService _userAvailabilitiesService;
        private readonly GroupsService _groupsService;
        private readonly GroupAvailabilitiesService _groupAvailabilitiesService;
        private readonly UserGroupsService _userGroupsService;
        private readonly RedisService _cache;

        public AvailabilityController(
            AvailabilitiesService availabilities, UserAvailabilitiesService userAvailabilities,
            GroupAvailabilitiesService groupAvailabilities, GroupsService groups, 
            UserGroupsService userGroups, RedisService cache)
        {
            _availabilitiesService = availabilities;
            _groupsService = groups;
            _groupAvailabilitiesService = groupAvailabilities;
            _userAvailabilitiesService = userAvailabilities;
            _userGroupsService = userGroups;
            _cache = cache;
        }

        [HttpGet("group/{gid}")]
        public async Task<IActionResult> GetGroup(string gid)
        {
            try
            {
                var group = await _groupsService.GetAsync(gid);

                if (group == null)
                    return NotFound($"Group with ID '{gid}' was not found.");

                return Ok(group);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Group: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the group." });
            }
        }

        [HttpGet("availability/{aid}")]
        public async Task<IActionResult> GetAvailability(string aid)
        {
            try
            {
                var availability = await _availabilitiesService.GetAsync(aid);

                if (availability == null)
                    return NotFound($"Availability with ID '{aid}' was not found.");

                return Ok(availability);
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
            try
            {
                var userAvailabilities = await _userAvailabilitiesService.GetAsync(uID);
                if (userAvailabilities == null || userAvailabilities.Availabilities == null)
                    return NotFound($"No availabilities found for user with UID: {uID}.");

                var availabilityDetails = new List<Availability>();

                foreach (var aid in userAvailabilities.Availabilities)
                {
                    try
                    {
                        var availability = await _availabilitiesService.GetAsync(aid);

                        if (availability != null) availabilityDetails.Add(availability);
                        else Console.WriteLine($"Availability with ID {aid} not found in MongoDB.");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing Availability ID '{aid}': {ex.Message}\n{ex.StackTrace}");
                    }
                }

                return Ok(availabilityDetails);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving User's Availabilities: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the availabilities." });
            }
        }

        [HttpPost("group")]
        public async Task<IActionResult> PostGroup([FromBody] Group newGroup)
        {
            try
            {
                var insertedGroup = await _groupsService.CreateAsync(newGroup);

                if (string.IsNullOrEmpty(insertedGroup.Organizer) || string.IsNullOrEmpty(insertedGroup.gID))
                {
                    var nullValue = string.IsNullOrEmpty(insertedGroup.Organizer) ? nameof(insertedGroup.Organizer) : nameof(insertedGroup.gID);
                    throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null or empty.");
                }

                await _userGroupsService.AddGroupAsync(insertedGroup.Organizer, insertedGroup.gID);
                await _groupAvailabilitiesService.CreateAsync(insertedGroup.gID);

                return CreatedAtAction(nameof(GetGroup), new { insertedGroup.gID }, insertedGroup);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Group: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while creating the group." });
            }
        }

        [HttpPost("availability")]
        public async Task<IActionResult> PostAvailability([FromBody] Availability newAvailability)
        {
            try
            {
                var insertedAvailability = await _availabilitiesService.CreateAsync(newAvailability);
                if (insertedAvailability == null || string.IsNullOrEmpty(insertedAvailability.UID) || string.IsNullOrEmpty(insertedAvailability.aID))
                    throw new InvalidOperationException("Failed to create availability or set required properties.");

                await _userAvailabilitiesService.AddAvailabilityAsync(insertedAvailability.UID, insertedAvailability.aID);
                await _groupAvailabilitiesService.AddAvailabilityAsync(insertedAvailability.gID, insertedAvailability.aID);

                return CreatedAtAction(nameof(GetAvailability), new { insertedAvailability.aID }, insertedAvailability);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Availability: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while creating the availability." });
            }
        }
    }
}