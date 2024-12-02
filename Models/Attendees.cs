using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models
{
    public class Attendees
    {
        [BsonId]
        public string Id { get; set; }
        public string[] Users { get; set; }
    }
}