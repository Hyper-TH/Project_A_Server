using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Meetings
{
    public class UserMeetings
    {
        [BsonId]
        [BsonElement("UID")]
        public string? UID { get; set; } = null!;

        [BsonElement("Meetings")]
        public string[] Meetings { get; set; } = [];
    }
}
