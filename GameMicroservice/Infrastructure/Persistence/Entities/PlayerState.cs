using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GameMicroservice.Domain;


namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents the snapshot state of a player in a game session.
    /// </summary>
    public class PlayerState
    {
        [BsonElement("playerId")]
        public string PlayerId { get; set; } = null!; // Reference to the user

        [BsonElement("lifeTotal")]
        public int LifeTotal { get; set; } = 20; // Remaining life points

        [BsonElement("manaPool")]
        public Dictionary<string, int> ManaPool { get; set; } = new()
        {
            { "White", 0 }, { "Blue", 0 }, { "Black", 0 }, { "Red", 0 }, { "Green", 0 }
        };

        [BsonElement("landsPlayedThisTurn")]
        public int LandsPlayedThisTurn { get; set; } // Number of lands played this turn

        [BsonElement("hasDrawnThisTurn")]
        public bool HasDrawnThisTurn { get; set; } // Whether the player has drawn this turn
    }
}