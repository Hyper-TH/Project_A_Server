using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models
{
    public class UserMeetings
    {
        [BsonId]
        public string? UID { get; set; } = null!;
        public string[] Meetings { get; set; }
    }
}
