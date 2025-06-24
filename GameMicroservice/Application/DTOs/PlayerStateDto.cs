namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// DTO representing the state of a player in a game session.
    /// </summary>
    public class PlayerStateDto
    {
        /// <summary>
        /// Identifier of the player.
        /// </summary>
        public string PlayerId { get; set; } = null!;

        /// <summary>
        /// Remaining life points.
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// List of card IDs in the player's hand.
        /// </summary>
        public IList<string> Hand { get; set; } = new List<string>();

    }
}