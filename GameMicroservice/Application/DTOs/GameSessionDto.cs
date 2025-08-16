using GameMicroservice.Domain;

namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO representing a game session.
    /// </summary>
    public class GameSessionDto
    {
        /// <summary>
        /// Unique identifier of the game session.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// ID of the first player.
        /// </summary>
        public string PlayerOneId { get; set; } = null!;

        /// <summary>
        /// ID of the second player.
        /// </summary>
        public string PlayerTwoId { get; set; } = null!;

        /// <summary>
        /// ID of the player whose turn it is.
        /// </summary>
        public string ActivePlayerId { get; set; } = null!;

        public int TurnNumber { get; set; } = 1;

        public List<PlayerStateDto> Players { get; set; } = new();

        /// <summary>
        /// Game zones: "library", "hand", "battlefield", "graveyard".
        /// </summary>
        public Dictionary<string, List<CardInGameDto>> Zones { get; set; } = new();

        public Phase CurrentPhase { get; set; } = Phase.Draw;

        public List<AvailableActionDto> AvailableActions { get; set; } = new();


    }
}
