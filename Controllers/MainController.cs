using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Models;
using Project_A_Server.Services.MongoDB.Utils;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services;
using System.Security.Cryptography;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Services.MongoDB.Availabilities;

// TODO: Change logic by taking in the entire document and updating it
// instead of directly updating the resource
namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly MeetingsService _meetingsService;
        private readonly AttendeesService _attendeesService;
        private readonly UserMeetingsService _userMeetingsService;
        private readonly UnregisterUsers _unregisterUsers;
        private readonly RedisService _cache;

        public MainController(MeetingsService meetingsService,
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
            if (string.IsNullOrEmpty(mid))
            {
                return BadRequest("Meeting ID cannot be null or empty.");
            }

            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(mid);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {mid} does not exist." });
                }

                var meeting = await _meetingsService.GetAsync(cachedDocId);

                if (meeting == null)
                {
                    return NotFound($"Meeting with ID '{mid}' was not found.");
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

        [Authorize]
        [HttpGet("meetings/{uid}")]
        public async Task<IActionResult> GetUserMeetings(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                var userMeetings = await _userMeetingsService.GetByUIDAsync(uid);

                if (userMeetings == null)
                {
                    return NotFound($"User with ID '{uid}' was not found.");
                }

                var meetingDetails = new List<Meeting>(); 

                foreach (var mid in userMeetings.Meetings)
                {
                    try
                    {
                        var cachedDocId = await _cache.GetCachedDocIdAsync(mid);
                        if (string.IsNullOrEmpty(cachedDocId))
                        {
                            Console.WriteLine($"Meeting ID '{mid}' does not exist in Redis.");
                            continue; // Skip this meeting if no docID is found
                        }

                        var meeting = await _meetingsService.GetAsync(cachedDocId);
                        if (meeting != null)
                        {
                            meetingDetails.Add(meeting); 
                        }
                        else
                        {
                            Console.WriteLine($"Meeting with docID '{cachedDocId}' not found in MongoDB.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing Meeting ID '{mid}': {ex.Message}\n{ex.StackTrace}");
                    }
                }

                return Ok(meetingDetails);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving User's Meetings: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving the meetings." });
            }
        }

        [Authorize]
        [HttpPut("register/{uID}/{mID}")]
        public async Task<IActionResult> RegisterMeeting(string uID, string mID)
        {
            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(mID);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {mID} does not exist." });
                }

                await _userMeetingsService.AddMeetingAsync(uID, mID);
                await _attendeesService.AddUserToMeetingAsync(uID, mID);

                return Ok(new { Message = "Registered meeting successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error registering Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while registering the meeting." });
            }
        }

        [Authorize]
        [HttpPut("unregister/{uID}/{mID}")]
        public async Task<IActionResult> Unregister(string uID, string mID)
        {
            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(mID);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {mID} does not exist." });
                }

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

        [Authorize]
        [HttpPost("meeting")]
        public async Task<IActionResult> Post([FromBody] Meeting newMeeting)
        {
            if (newMeeting == null)
            {
                return BadRequest("Invalid meeting data");
            }

            try
            {
                string mID;
                do
                {
                    mID = Guid.NewGuid().ToString("N");
                    var cachedDocId = await _cache.GetCachedDocIdAsync(mID);

                    if (string.IsNullOrEmpty(cachedDocId))
                    {
                        newMeeting.mID = mID;
                        break;
                    }

                    Console.WriteLine($"Collision detected for mID: {mID}. Generating a new one.");
                }
                while (true);

                newMeeting.mID = mID;
                var newAttendees = new Attendees
                {
                    mID = newMeeting.mID,
                    Users = Array.Empty<string>()
                };

                await _meetingsService.CreateAsync(newMeeting);
                await _attendeesService.CreateAsync(newAttendees);
                await _userMeetingsService.AddMeetingAsync(newMeeting.Organizer, newMeeting.mID);

                var insertedMeeting = await _meetingsService.GetAsync(mID);
                if (insertedMeeting?.Id == null)
                {
                    throw new InvalidOperationException("Failed to retrieve meeting ID after insertion.");
                }
                await _cache.CacheIDAsync(mID, insertedMeeting.Id);

                return CreatedAtAction(nameof(GetMeeting), new { newMeeting.mID }, newMeeting);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while creating the meeting." });
            }
        }

        [Authorize]
        [HttpDelete("meeting/{uID}/{mID}")]
        public async Task<IActionResult> DeleteMeeting(string uID, string mID)
        {
            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(mID);
                if (string.IsNullOrEmpty(cachedDocId))
                {
                    return Conflict(new { Message = $"ID {mID} does not exist." });
                }

                await _unregisterUsers.UnregisterMeetingAsync(mID);
                await _userMeetingsService.RemoveMeetingAsync(uID, mID);
                await _cache.RemoveCachedIDAsync(mID);

                return Ok(new { Message = "Meeting removed successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error removing Meeting: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while removing the meeting." });
            }
        }
    }
}
