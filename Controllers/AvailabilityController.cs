﻿using Microsoft.AspNetCore.Mvc;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Models.Availabilities;
using Microsoft.AspNetCore.Authorization;
using Project_A_Server.Utils;

namespace Project_A_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilitiesService _availabilitiesService;
        private readonly UserAvailabilitiesService _userAvailabilitiesService;
        private readonly GroupsService _groupsService;
        private readonly RedisService _cache;

        public AvailabilityController(
            AvailabilitiesService availabilities, UserAvailabilitiesService userAvailabilities, 
            GroupsService groups, RedisService cache)
        {
            _availabilitiesService = availabilities;
            _groupsService = groups;
            _userAvailabilitiesService = userAvailabilities;
            _cache = cache;
        }

        [HttpGet("group/{gid}")]
        public async Task<IActionResult> GetGroup(string gid)
        {
            if (string.IsNullOrEmpty(gid))
                return BadRequest("Group ID cannot be null or empty.");

            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(gid);
                if (string.IsNullOrEmpty(cachedDocId))
                    return Conflict(new { Message = $"ID {gid} does not exist." });

                var group = await _groupsService.GetByIdAsync(cachedDocId);

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

        [HttpPost("group")]
        public async Task<IActionResult> PostGroup([FromBody] Group newGroup)
        {
            if (newGroup == null)
                return BadRequest("Invalid group data");

            try
            {
                string gID;
                do
                {
                    gID = GuidGenerator.Generate();
                    var cachedDocId = await _cache.GetCachedDocIdAsync(gID);

                    if (string.IsNullOrEmpty(cachedDocId))
                    {
                        newGroup.gID = gID;
                        break;
                    }

                    Console.WriteLine($"Collision detected for gID: {gID}. Generating a new one.");
                }
                while (true);

                newGroup.gID = gID;

                await _groupsService.CreateAsync(newGroup);

                var insertedGroup = await _groupsService.GetAsync(gID);
                if (insertedGroup?.Id == null)
                    throw new InvalidOperationException("Failed to retrieve Object ID after insertion.");

                await _cache.CacheIDAsync(gID, insertedGroup.Id);

                return CreatedAtAction(nameof(GetGroup), new { newGroup.gID }, newGroup);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating Group: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "An error occured while creating the group." });
            }
        }

        [HttpGet("availability/{aid}")]
        public async Task<IActionResult> GetAvailability(string aid)
        {
            if (string.IsNullOrEmpty(aid))
                return BadRequest("Availability ID cannot be null or empty.");

            try
            {
                var cachedDocId = await _cache.GetCachedDocIdAsync(aid);
                if (string.IsNullOrEmpty(cachedDocId))
                    return Conflict(new { Message = $"ID {aid} does not exist." });

                var availability = await _availabilitiesService.GetByIdAsync(cachedDocId);

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
            if (string.IsNullOrEmpty(uID))
                return BadRequest("User ID cannot be null or empty.");

            try
            {
                var userAvailabilities = await _userAvailabilitiesService.GetByUIDAsync(uID);

                if (userAvailabilities == null)
                    return NotFound($"User with ID '{uID}' was not found.");

                var availabilityDetails = new List<Availability>();

                foreach (var aid in userAvailabilities.Availabilities)
                {
                    try
                    {
                        var cachedDocId = await _cache.GetCachedDocIdAsync(aid);
                        if (string.IsNullOrEmpty(cachedDocId))
                        {
                            Console.WriteLine($"Availability ID '{aid}' does not exist in Redis.");
                            continue; 
                        }

                        var availability = await _availabilitiesService.GetByIdAsync(cachedDocId);

                        if (availability != null) availabilityDetails.Add(availability);
                        else Console.WriteLine($"Availability with docID '{cachedDocId}' not found in MongoDB.");
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
        public async Task<IActionResult> PostAvailability([FromBody] Availability newAvailability)
        {
            if (newAvailability == null)
                return BadRequest("Invalid availability data");

            try
            {
                string aID;
                do
                {
                    aID = GuidGenerator.Generate();
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
                await _userAvailabilitiesService.AddAvailabilityAsync(newAvailability.UID, newAvailability.aID);

                var insertedAvailability = await _groupsService.GetByIdAsync(aID);
                if (insertedAvailability?.Id == null)
                    throw new InvalidOperationException("Failed to retrieve ObjectID after insertion.");

                await _cache.CacheIDAsync(aID, insertedAvailability.Id);

                return CreatedAtAction(nameof(GetAvailability), new { newAvailability.aID }, newAvailability);
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