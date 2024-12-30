using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Services.Redis;
using System.Security.Cryptography;

namespace Project_A_Server.Utils
{
    public class RemoveGroup
    {
        private readonly UserGroupsService _userGroupsService;
        private readonly GroupAvailabilitiesService _groupAvailabilitiesService;
        private readonly AvailabilitiesService _availabilitiesService;
        private readonly GroupsService _groupsService;
        private readonly RedisService _cache;

        public RemoveGroup(UserGroupsService userGroupsService, GroupAvailabilitiesService groupAvailabilitiesService,
                            AvailabilitiesService availabilitiesService, GroupsService groupsService, RedisService cache)
        {
            _userGroupsService = userGroupsService;
            _groupAvailabilitiesService = groupAvailabilitiesService;
            _availabilitiesService=availabilitiesService;
            _groupsService = groupsService;
            _cache = cache;
        }

        public async Task DeleteGroupAsync(string gid)
        {
            var group = await _groupsService.GetAsync(gid) ?? throw new InvalidOperationException($"Group {gid} not found.");

            await UnregisterRecursively(gid, group.Users.ToList());
        }

        private async Task UnregisterRecursively(string gid, List<string> users)
        {
            if (users.Count == 0)
            {
                // Base case: Remove availability from GroupAvailabilities
                await RemoveAvailabilities(gid);

                return;
            }

            var uid = users.First();
            await _userGroupsService.RemoveGroupAsync(uid, gid);

            await UnregisterRecursively(gid, users.Skip(1).ToList());
        }

        private async Task RemoveAvailabilities(string gid)
        {
            var groupAvailabilities = await _groupAvailabilitiesService.GetAllAsync(gid);

            if (groupAvailabilities == null)
            {
                await _groupAvailabilitiesService.RemoveAsync(gid);

                return;
            }

            foreach (var aid in groupAvailabilities.Availabilities)
            {
                try
                {
                    await _availabilitiesService.RemoveAsync(aid);
                    await _cache.RemoveCachedIDAsync(aid);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to remove availability {aid}: {ex.Message}");
                }
            }
        }
    }
}