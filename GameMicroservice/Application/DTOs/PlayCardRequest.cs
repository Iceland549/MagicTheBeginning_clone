namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// Request to play a card in a game session.
    /// </summary>
    public class PlayCardRequest
    {
        /// <summary>
        /// Name of the card to play.
        /// </summary>
        public string CardName { get; set; } = null!;
    }
}