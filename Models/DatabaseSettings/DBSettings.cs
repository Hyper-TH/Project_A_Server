namespace Project_A_Server.Models.DatabaseSettings
{
    public class DBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string MeetingsCollectionName { get; set; } = null!;
        public string AttendeesCollectionName { get; set; } = null!;
        public string UserMeetingsCollectionName { get; set; } = null!;
        public string UserAvailabilitiesCollectionName { get; set; } = null!;
    }
}
