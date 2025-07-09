namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// Request to start a new game session.
    /// </summary>
    public class StartGameRequest
    {
        /// <summary>
        /// ID of the first player.
        /// </summary>
        public string PlayerOneId { get; set; } = null!;

        /// <summary>
        /// ID of the second player.
        /// </summary>
        public string PlayerTwoId { get; set; } = null!;

        public string DeckIdP1 { get; set; } = null!;
        public string DeckIdP2 { get; set; } = null!;
    }
}