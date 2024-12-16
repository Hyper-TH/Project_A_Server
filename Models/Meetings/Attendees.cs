using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Meetings
{
    public class Attendees
    {
        [BsonId]
        [BsonElement("mID")]
        public string? mID { get; set; }

        [BsonElement("Users")]
        public string[] Users { get; set; } = [];
    }
}