using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Project_A_Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UID")]
        public string UID { get; set; } = null!;

        [BsonElement("Username")]
        [JsonPropertyName("Username")]
        public string Username { get; set; } = null!;

        [BsonElement("PasswordHash")]
        [JsonPropertyName("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [BsonElement("Location")]
        public string Location { get; set; }
    }
}
