using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case for retrieving the state of a game session.
    /// </summary>
    public class GetGameStateUseCase
    {
        private readonly IGameSessionRepository _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGameStateUseCase"/> class.
        /// </summary>
        /// <param name="repo">The game session repository.</param>
        public GetGameStateUseCase(IGameSessionRepository repo) => _repo = repo;

        /// <summary>
        /// Executes the use case to retrieve a game session by ID.
        /// </summary>
        /// <param name="gameId">The ID of the game session.</param>
        /// <returns>The game session DTO.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game session is not found.</exception>
        public async Task<GameSessionDto> ExecuteAsync(string gameId)
        {
            var game = await _repo.GetByIdAsync(gameId)
                       ?? throw new KeyNotFoundException("Game session not found");
            return game;
        }
    }
}