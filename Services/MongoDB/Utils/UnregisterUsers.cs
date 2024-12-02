namespace Project_A_Server.Services.MongoDB.Utils
{
    public class UnregisterUsers
    {
        private readonly UserMeetingsService _userMeetingsService;
        private readonly AttendeesService _attendeesService;
        private readonly MeetingsService _meetingsService;

        public UnregisterUsers(UserMeetingsService userMeetingsService,
                               AttendeesService attendeesService,
                               MeetingsService meetingsService)
        {
            _userMeetingsService = userMeetingsService;
            _attendeesService = attendeesService;
            _meetingsService = meetingsService;
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
                await _meetingsService.RemoveAsync(mID);

                return;
            }

            var uID = users.First();
            await _userMeetingsService.RemoveMeetingAsync(uID, mID);

            await UnregisterRecursively(mID, users.Skip(1).ToList());
        }
    }
}
