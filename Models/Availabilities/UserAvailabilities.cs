using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Project_A_Server.Models.Availabilities
{
    public class UserAvailabilities
    {
        [BsonId]
        public string? UID { get; set; } = null!;
        public string[] Availabilities { get; set; }
    }
}
