using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Threading.Tasks;

namespace GameMicroservice.Application.Interfaces
{
    /// <summary>
    /// Interface for managing game session persistence.
    /// </summary>
    public interface IGameSessionRepository
    {
        /// <summary>
        /// Creates a new game session for two players.
        /// </summary>
        /// <param name="playerOneId">ID of the first player.</param>
        /// <param name="playerTwoId">ID of the second player.</param>
        /// <returns>The created game session entity.</returns>
        Task<GameSession> CreateAsync(string playerOneId, string playerTwoId);

        /// <summary>
        /// Retrieves a game session by its ID.
        /// </summary>
        /// <param name="gameId">The ID of the game session.</param>
        /// <returns>The game session entity, or null if not found.</returns>
        Task<GameSession?> GetByIdAsync(string gameId);

        /// <summary>
        /// Updates an existing game session.
        /// </summary>
        /// <param name="session">The game session entity to update.</param>
        Task UpdateAsync(GameSession session);

        /// <summary>
        /// Updates a game session by playing a card.
        /// </summary>
        /// <param name="gameId">The ID of the game session.</param>
        /// <param name="cardName">The name of the card to play.</param>
        Task PlayCardAsync(string gameId, string cardName);
    }
}