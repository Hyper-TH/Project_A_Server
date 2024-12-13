using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Project_A_Server.Models.Availabilities
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? gID { get; set; }
        public string Name { get; set; } = null!;
        public string Organizer { get; set; } = null!;
        public string Description { get; set; }
        public string[] Users { get; set; }
    }
}
