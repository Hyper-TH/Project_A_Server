using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// TODO: Modify Users[] to have it as [{id, color},{..}]
namespace Project_A_Server.Models.Availabilities
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("gID")]
        public string? gID { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("Organizer")]
        public string Organizer { get; set; } = null!;

        [BsonElement("Description")]
        public string Description { get; set; } = "";

        [BsonElement("Users")]
        public string[] Users { get; set; } = [];
    }
}
