﻿using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

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
        private readonly RemoveGroup _removeGroup;

        public AvailabilityController(
            AvailabilitiesService availabilities, UserAvailabilitiesService userAvailabilities,
            GroupAvailabilitiesService groupAvailabilities, GroupsService groups, 
            UserGroupsService userGroups, RedisService cache, RemoveGroup removeGroup)
        {
            _availabilitiesService = availabilities;
            _groupsService = groups;
            _groupAvailabilitiesService = groupAvailabilities;
            _userAvailabilitiesService = userAvailabilities;
            _userGroupsService = userGroups;
            _cache = cache;
            _removeGroup = removeGroup;
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

        [HttpGet("groups/{uid}")]
        public async Task<IActionResult> GetGroups(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                    return BadRequest("User ID cannot be null or empty.");

                var groups = await _userGroupsService.GetAllAsync(uid);

                var groupData = new List<object>();

                foreach (var group in groups.Groups)
                {
                    var groupDetails = await _groupsService.GetAsync(group.gID);

                    if (groupDetails == null)
                    {
                        Console.WriteLine($"Group with gID {group.gID} not found.");
                        continue;
                    }


                    groupData.Add(new
                    {
                        group.gID,
                        groupDetails.Name,
                        groupDetails.Organizer,
                        groupDetails.Description,
                    });
                }

                return Ok(groupData);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Groups: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving groups." });
            }
        }

        [HttpGet("userGroups/{uid}")]
        public async Task<IActionResult> GetUserGroupAvailabilities(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                    return BadRequest("User ID cannot be null or empty.");

                var userGroups = await _userGroupsService.GetAllAsync(uid);

                if (userGroups == null)
                    return NotFound($"User with ID '{uid}' was not found.");

                var groupData = new List<object>();

                foreach (var group in userGroups.Groups)
                {
                    var groupDetails = await _groupsService.GetAsync(group.gID);

                    if (groupDetails == null)
                    {
                        Console.WriteLine($"Group with gID {group.gID} not found.");
                        continue;
                    }

                    var availabilityDetails = new List<object>();
                    foreach (var aid in group.Availabilities)
                    {
                        var availability = await _availabilitiesService.GetAsync(aid);

                        if (availability != null)
                        {
                            availabilityDetails.Add(availability);
                        }
                        else
                        {
                            Console.WriteLine($"Availability with ID {aid} not found.");
                        }
                    }

                    groupData.Add(new
                    {
                        Group = groupDetails,
                        Availabilities = availabilityDetails
                    });
                }

                return Ok(groupData);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Groups: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving groups." });
            }
        }

        [HttpPut("group/{uid}/{gid}")]
        public async Task<IActionResult> AddUserToGroup(string uid, string gid)
        {
            if (uid == null || gid == null)
            {
                var nullValue = uid == null ? nameof(uid) : nameof(gid);

                throw new ArgumentNullException(nullValue, $"{nullValue} cannot be null.");
            }

            try
            {
                await _groupsService.AddUserToGroupAsync(uid, gid);
                await _userGroupsService.AddGroupAsync(uid, gid);

                return Ok(new { Message = "Registered group successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Group: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while creating the group." });
            }
        }

        [HttpGet("groupAvailabilities/{gid}")]
        public async Task<IActionResult> GetGroupAvailabilities(string gid)
        {
            try
            {
                if (string.IsNullOrEmpty(gid))
                    return BadRequest("User ID cannot be null or empty.");

                var group = await _groupAvailabilitiesService.GetAllAsync(gid);

                if (group == null)
                    return NotFound($"Group with ID '{gid}' was not found.");

                var dateTimes = new List<object>();

                foreach (var aid in group.Availabilities)
                {
                    var dt = await _availabilitiesService.GetAsync(aid);

                    if (dt != null)
                    {
                        dateTimes.Add(new
                        {
                            dt.Date,
                            dt.Start,
                            dt.End,
                            dt.Timezone,

                        });
                    }
                    else
                    {
                        Console.WriteLine($"Availability with ID {aid} not found.");
                    }
                }

                var groupAvailabilities = AvailabilityMapper.MapAvailabilities(dateTimes);

                return Ok(groupAvailabilities);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving Groups: {ex.Message}\n{ex.StackTrace}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while retrieving groups." });
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

        [HttpGet("availabilities/{uid}")]
        public async Task<IActionResult> GetUserAvailabilities(string uid)
        {
            try
            {
                var userAvailabilities = await _userAvailabilitiesService.GetAsync(uid);
                if (userAvailabilities == null || userAvailabilities.Availabilities == null)
                    return NotFound($"No availabilities found for user with UID: {uid}.");

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
        
        [HttpDelete("group/{gid}")]
        public async Task<IActionResult> DeleteGroup(string gid)
        {
            // TODO: UserAvailabilities might become redundant
            // because of the new structure of UserGroups (check mongoDB)
            try
            {
                await _removeGroup.DeleteGroupAsync(gid);
                await _groupsService.RemoveAsync(gid);
                await _cache.RemoveCachedIDAsync(gid);

                return Ok(new { Message = "Group removed successfully." });
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine($"Error deleting Group: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occurred while deleting the Group." });
            }
        }

        [HttpPost("availability")]
        public async Task<IActionResult> PostAvailability([FromBody] Availability newAvailability)
        {
            try
            {
                var insertedAvailability = await _availabilitiesService.CreateAsync(newAvailability);
                if (string.IsNullOrEmpty(insertedAvailability.gID) || string.IsNullOrEmpty(insertedAvailability.UID) || string.IsNullOrEmpty(insertedAvailability.aID))
                    throw new InvalidOperationException("Failed to create availability or set required properties.");

                await _userAvailabilitiesService.AddAvailabilityAsync(insertedAvailability.UID, insertedAvailability.aID);
                await _userGroupsService.AddAvailabilityAsync(insertedAvailability.UID, insertedAvailability.gID, insertedAvailability.aID);
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

        [HttpDelete("availability/{uid}/{gid}/{aid}")]
        public async Task<IActionResult> DeleteAvailability(string uid, string gid, string aid)
        {
            try
            {
                await _availabilitiesService.RemoveAsync(aid);
                await _userAvailabilitiesService.RemoveAvailabilityAsync(uid, aid);
                await _groupAvailabilitiesService.RemoveAvailabilityAsync(gid, aid);
                await _cache.RemoveCachedIDAsync(aid);

                return Ok(new { Message = "Availability removed successfully." });

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting Availability : {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while deleting the availability." });
            }
        }
    }
}