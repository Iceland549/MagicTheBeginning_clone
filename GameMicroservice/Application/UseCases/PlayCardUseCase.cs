using GameMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case for playing a card in a game session.
    /// </summary>
    public class PlayCardUseCase
    {
        private readonly IGameSessionRepository _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayCardUseCase"/> class.
        /// </summary>
        /// <param name="repo">The game session repository.</param>
        public PlayCardUseCase(IGameSessionRepository repo) => _repo = repo;

        /// <summary>
        /// Executes the use case to play a card in a game session.
        /// </summary>
        /// <param name="gameId">The ID of the game session.</param>
        /// <param name="cardName">The name of the card to play.</param>
        public Task ExecuteAsync(string gameId, string cardName) =>
            _repo.PlayCardAsync(gameId, cardName);
    }
}