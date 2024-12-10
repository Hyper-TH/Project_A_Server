using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Project_A_Server.Models.Availabilities
{
    public class UserAvailabilities
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? aID { get; set; }
        public DateTime? Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Timezone { get; set; }
    }
}
