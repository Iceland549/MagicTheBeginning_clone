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
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("playerId")]
        public string PlayerId { get; set; } = null!; // Reference to the user

        [BsonElement("lifeTotal")]
        public int LifeTotal { get; set; } = 20; // Remaining life points

        [BsonElement("manaPool")]
        public Dictionary<Color, int> ManaPool { get; set; } = new()
        {
            { Color.White, 0 }, { Color.Blue, 0 }, { Color.Black, 0 }, { Color.Red, 0 }, { Color.Green, 0 }
        }; // Available mana by color

        [BsonElement("landsPlayedThisTurn")]
        public int LandsPlayedThisTurn { get; set; } // Number of lands played this turn

        [BsonElement("hasDrawnThisTurn")]
        public bool HasDrawnThisTurn { get; set; } // Whether the player has drawn this turn
    }
}