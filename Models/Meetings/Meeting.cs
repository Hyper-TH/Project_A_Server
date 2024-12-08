using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Project_A_Server.Models.Meetings
{
    public class Meeting
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? mID { get; set; }
        [BsonElement("Name")]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = null!;
        public string Organizer { get; set; } = null!;
        public string Description { get; set; } = null!;

        [BsonElement("DateTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateTime { get; set; }
        public string Timezone { get; set; } = null!;
    }
}
