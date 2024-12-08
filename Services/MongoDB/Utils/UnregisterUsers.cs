using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Services.Redis;

namespace Project_A_Server.Services.MongoDB.Utils
{
    public class UnregisterUsers
    {
        private readonly UserMeetingsService _userMeetingsService;
        private readonly AttendeesService _attendeesService;
        private readonly MeetingsService _meetingsService;
        private readonly RedisService _cache;

        public UnregisterUsers(UserMeetingsService userMeetingsService,
                               AttendeesService attendeesService,
                               MeetingsService meetingsService,
                               RedisService redisService)
        {
            _userMeetingsService = userMeetingsService;
            _attendeesService = attendeesService;
            _meetingsService = meetingsService;
            _cache = redisService;
        }

        public async Task UnregisterMeetingAsync(string mID)
        {
            var meeting = await _attendeesService.GetAsync(mID);
            if (meeting == null)
            {
                throw new InvalidOperationException($"Meeting {mID} not found.");
            }

            await UnregisterRecursively(mID, meeting.Users.ToList());
        }

        private async Task UnregisterRecursively(string mID, List<string> users)
        {
            if (users.Count == 0)
            {
                // Base case: Remove meeting after all users are unregistered
                await _attendeesService.RemoveAsync(mID);

                var cachedDocId = await _cache.GetCachedDocIdAsync(mID);
                await _meetingsService.RemoveAsync(cachedDocId);    

                return;
            }

            var uID = users.First();
            await _userMeetingsService.RemoveMeetingAsync(uID, mID);

            await UnregisterRecursively(mID, users.Skip(1).ToList());
        }
    }
}
