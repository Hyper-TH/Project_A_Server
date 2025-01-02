using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Project_A_Server.Models.Availabilities
{
    public class Availability
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("aID")]
        public string? aID { get; set; }

        [BsonElement("UID")]
        public string UID { get; set; } = null!;

        [BsonElement("gID")]
        public string gID { get; set; } = null!;

        [BsonElement("DateTime")]
        public DateTime Date { get; set; }

        [BsonElement("Start")]
        public string Start { get; set; } = null!;

        [BsonElement("End")]
        public string End { get; set; } = null!;

        [BsonElement("Timezone")]
        public string Timezone { get; set; } = null!;
    }
}
