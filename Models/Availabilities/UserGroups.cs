﻿using MongoDB.Bson.Serialization.Attributes;

// TODO: Color logic
namespace Project_A_Server.Models.Availabilities
{
    public class UserGroups
    {
        [BsonId]
        [BsonElement("UID")]
        public string? UID { get; set; }

        [BsonElement("Groups")]
        public Group[] Groups { get; set; } = [];

        public class Group
        {
            [BsonElement("gID")]
            public string gID { get; set; } = string.Empty;

            [BsonElement("Color")]
            public string color { get; set; } = string.Empty;

            [BsonElement("Availabilities")]
            public string[] Availabilities { get; set; } = [];
        }
    }
}
