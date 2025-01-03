﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Project_A_Server.Models.Availabilities
{
    public class UserAvailabilities
    {
        [BsonId]
        [BsonElement("UID")]
        public string? UID { get; set; } = null!;

        [BsonElement("Availabilities")]
        public string[] Availabilities { get; set; } = [];
    }
}
