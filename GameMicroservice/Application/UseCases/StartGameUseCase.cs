using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case for starting a new game session between two players.
    /// </summary>
    public class StartGameUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartGameUseCase"/> class.
        /// </summary>
        /// <param name="repo">The game session repository.</param>
        /// <param name="mapper">The AutoMapper instance for mapping entities to DTOs.</param>
        public StartGameUseCase(IGameSessionRepository repo, IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Executes the use case to start a game session.
        /// </summary>
        /// <param name="p1">ID of the first player.</param>
        /// <param name="p2">ID of the second player.</param>
        /// <returns>The created game session DTO.</returns>
        public async Task<GameSessionDto> ExecuteAsync(string p1, string p2, string deckId)
        {
            var gameSession = await _repo.CreateAsync(p1, p2, deckId);
            return _mapper.Map<GameSessionDto>(gameSession);
        }
    }
}