using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Utils;
using System.Security.Cryptography;

// TODO: Change logic by taking in the entire document and updating it
// instead of directly updating the resource
namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly MeetingsService _meetingsService;
        private readonly AttendeesService _attendeesService;
        private readonly UserMeetingsService _userMeetingsService;
        private readonly UnregisterUsers _unregisterUsers;
        private readonly RedisService _cache;

        public MeetingController(MeetingsService meetingsService,
                                    AttendeesService attendeesService, UserMeetingsService userMeetings,
                                    UnregisterUsers unregisterUsers, RedisService redisService)
        {
            _meetingsService = meetingsService;
            _attendeesService = attendeesService;
            _userMeetingsService = userMeetings;
            _unregisterUsers = unregisterUsers;
            _cache = redisService;
        }

        [HttpGet("meeting/{mid}")]
        public async Task<IActionResult> GetMeeting(string mid)
        {
            try { 
                var meeting = await _meetingsService.GetAsync(mid);

                if (meeting == null)
                    return NotFound($"Meeting with ID '{mid}' was not found.");

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Meeting: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the meeting." });
            }
        }

        [HttpGet("meetings/{uid}")]
        public async Task<IActionResult> GetUserMeetings(string uid)
        {
            try
            {
                var userMeetings = await _userMeetingsService.GetAsync(uid);
                if (userMeetings == null)
                    return NotFound($"No meetings found for user with UID: {uid}.");

                var meetingDetails = new List<Meeting>(); 

                foreach (var mid in userMeetings.Meetings)
                {
                    try
                    {
                        var meeting = await _meetingsService.GetAsync(mid);

                        if (meeting != null) meetingDetails.Add(meeting);
                        else Console.WriteLine($"Meeting with ID '{mid}' not found in MongoDB.");

                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing Meeting ID '{mid}': {ex.Message}\n{ex.StackTrace}");
                    }
                }

                return Ok(meetingDetails);
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
                Console.Error.WriteLine($"Error retrieving User's Meetings: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the meetings." });
            }
        }

        [HttpPut("register/{uID}/{mID}")]
        public async Task<IActionResult> RegisterMeeting(string uid, string mid)
        {
            try
            {
                await _userMeetingsService.AddMeetingAsync(uid, mid);
                await _attendeesService.AddUserToMeetingAsync(uid, mid);

                return Ok(new { Message = "Registered meeting successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error registering Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while registering the meeting." });
            }
        }

        [HttpPut("unregister/{uID}/{mID}")]
        public async Task<IActionResult> Unregister(string uID, string mID)
        {
            try
            {
                await _userMeetingsService.RemoveMeetingAsync(uID, mID);
                await _attendeesService.RemoveUserFromMeetingAsync(uID, mID);

                return Ok(new { Message = "Unregistered meeting successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error unregistering Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while unregistering the meeting." });
            }
        }

        [HttpPost("meeting")]
        public async Task<IActionResult> PostMeeting([FromBody] Meeting newMeeting)
        {
            try
            {
                var insertedMeeting = await _meetingsService.CreateAsync(newMeeting);
                if (insertedMeeting == null || string.IsNullOrEmpty(insertedMeeting.mID))
                    throw new InvalidOperationException("Failed to create availability or set required properties.");

                await _attendeesService.CreateAsync(insertedMeeting.mID);
                await _userMeetingsService.AddMeetingAsync(insertedMeeting.Organizer, insertedMeeting.mID);

                return CreatedAtAction(nameof(GetMeeting), new { insertedMeeting.mID }, insertedMeeting);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while creating the Meeting." });
            }
        }

        [HttpDelete("meeting/{uID}/{mID}")]
        public async Task<IActionResult> DeleteMeeting(string uID, string mID)
        {
            try
            {
                await _unregisterUsers.UnregisterMeetingAsync(mID);
                await _userMeetingsService.RemoveMeetingAsync(uID, mID);
                await _cache.RemoveCachedIDAsync(mID);

                return Ok(new { Message = "Meeting removed successfully." });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while deleting the Meeting." });
            }
        }
    }
}