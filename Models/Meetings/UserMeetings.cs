using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Meetings
{
    public class UserMeetings
    {
        [BsonId]
        public string? UID { get; set; } = null!;
        public string[] Meetings { get; set; }
    }
}
