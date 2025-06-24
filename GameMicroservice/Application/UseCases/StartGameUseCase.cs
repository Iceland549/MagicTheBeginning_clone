using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case for starting a new game session between two players.
    /// </summary>
    public class StartGameUseCase
    {
        private readonly IGameSessionRepository _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartGameUseCase"/> class.
        /// </summary>
        /// <param name="repo">The game session repository.</param>
        public StartGameUseCase(IGameSessionRepository repo) => _repo = repo;

        /// <summary>
        /// Executes the use case to start a game session.
        /// </summary>
        /// <param name="p1">ID of the first player.</param>
        /// <param name="p2">ID of the second player.</param>
        /// <returns>The created game session DTO.</returns>
        public Task<GameSessionDto> ExecuteAsync(string p1, string p2) =>
            _repo.CreateAsync(p1, p2);
    }
}