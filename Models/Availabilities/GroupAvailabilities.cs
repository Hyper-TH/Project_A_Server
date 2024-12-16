using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Availabilities
{
    public class GroupAvailabilities
    {
        [BsonId]
        [BsonElement("gID")]
        public int gID { get; set; }

        [BsonElement("Availabilities")]
        public string[] Availabilities { get; set; } = [];
    }
}
