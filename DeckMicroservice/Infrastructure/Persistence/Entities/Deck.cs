using DeckMicroservice.Infrastructure.Persistence.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DeckMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents a deck built by a player.
    /// </summary>
    public class Deck
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;                 // MongoDB ObjectId

        [BsonElement("ownerId")]
        public string OwnerId { get; set; } = null!;           // User reference (GUID)

        [BsonElement("name")]
        public string Name { get; set; } = null!;              // Deck name

        [BsonElement("cards")]
        public List<DeckCard> Cards { get; set; } = new();     // List of cards and quantities

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Creation date
    }
}
