using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents the snapshot state of a player in a game session.
    /// </summary>
    public class PlayerState
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerId { get; set; } = null!;            // Reference to the user

        [BsonElement("health")]
        public int Health { get; set; }                          // Remaining life points

        [BsonElement("hand")]
        public List<string> Hand { get; set; } = new();         // IDs of cards in hand

        // ✨ Future options for game state: Mana, Board, Graveyard...
    }
}