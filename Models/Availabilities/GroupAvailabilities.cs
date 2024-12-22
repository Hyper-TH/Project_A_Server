using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Availabilities
{
    public class GroupAvailabilities
    {
        [BsonId]
        [BsonElement("gID")]
        public string? gID { get; set; } = null!;

        [BsonElement("Availabilities")]
        public string[] Availabilities { get; set; } = [];
    }
}
