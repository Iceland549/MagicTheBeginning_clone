using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;

namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents an active or completed game session.
    /// </summary>
    public class GameSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;                  // MongoDB ObjectId

        [BsonElement("playerOneId")]
        public string PlayerOneId { get; set; } = null!;        // Reference to the first player

        [BsonElement("playerTwoId")]
        public string PlayerTwoId { get; set; } = null!;        // Reference to the second player

        [BsonElement("activePlayerId")]
        public string ActivePlayerId { get; set; } = null!;

        [BsonElement("isPlayerTwoAI")]
        public bool IsPlayerTwoAI { get; set; } = false;        // Indicates if player two is an AI

        [BsonElement("startedAt")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow; // Start date of the session

        [BsonElement("players")]
        public List<PlayerState> Players { get; set; } = new(); // State of each player

        [BsonElement("currentState")]
        public string CurrentState { get; set; } = null!;       // e.g., "InProgress", "Finished"

        [BsonElement("zones")]
        public Dictionary<string, List<CardInGame>> Zones { get; set; } = new(); // Each player has 4 zones: library, hand, battlefield, graveyard

        [BsonElement("currentPhase")]
        public Phase CurrentPhase { get; set; } = Phase.Draw;

        [BsonElement("turnNumber")]
        public int TurnNumber { get; set; } = 1;


    }
}