using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Application.Interfaces
{
    /// <summary>
    /// Repository interface for GameSession persistence.
    /// Contains no business rules or game logic.
    /// </summary>
    public interface IGameSessionRepository
    {
        /// <summary>
        /// Retrieves a game session by its unique ID.
        /// </summary>
        /// <param name="id">The game session ID.</param>
        /// <returns>The GameSession entity or null if not found.</returns>
        Task<GameSession?> GetByIdAsync(string id);

        /// <summary>
        /// Returns all existing game sessions.
        /// </summary>
        Task<List<GameSession>> ListAllAsync();

        /// <summary>
        /// Persists a new GameSession in the database.
        /// </summary>
        /// <param name="session">Fully initialized GameSession entity.</param>
        Task CreateAsync(GameSession session);
        /// <summary>
        /// Updates an existing GameSession.
        /// </summary>
        /// <param name="session">The updated GameSession entity.</param>
        Task UpdateAsync(GameSession session);

        /// <summary>
        /// Deletes a game session by ID.
        /// </summary>
        /// <param name="id">The session ID to delete.</param>
        Task DeleteAsync(string id);
    }
}
