using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
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
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGameStateUseCase"/> class.
        /// </summary>
        /// <param name="repo">The game session repository.</param>
        /// <param name="mapper">The AutoMapper instance for mapping entities to DTOs.</param>
        public GetGameStateUseCase(IGameSessionRepository repo, IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Executes the use case to retrieve a game session by ID.
        /// </summary>
        /// <param name="gameId">The ID of the game session.</param>
        /// <returns>The game session DTO.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the game session is not found.</exception>
        public async Task<GameSessionDto> ExecuteAsync(string gameId, string playerId)
        {
            var game = await _repo.GetByIdAsync(gameId)
                       ?? throw new KeyNotFoundException("Game session not found");
            var dto = _mapper.Map<GameSessionDto>(game);

            // Génère dynamiquement les actions possibles
            dto.AvailableActions = GetAvailableActions(game, playerId);

            return dto;
        }

        private List<AvailableActionDto> GetAvailableActions(GameSession game, string playerId)
        {
            var actions = new List<AvailableActionDto>();

            if (game.ActivePlayerId != playerId)
                return actions; // Pas le tour du joueur

            switch (game.CurrentPhase)
            {
                case Phase.Draw:
                    actions.Add(new AvailableActionDto { Label = "Piocher", Type = "Draw", Disabled = false });
                    break;
                case Phase.Main:
                    var player = game.Players.Find(p => p.PlayerId == playerId);
                    var canPlayLand = player != null && player.LandsPlayedThisTurn < 1 &&
                        (game.Zones[$"{playerId}_hand"]?.Any(c => c.TypeLine != null && c.TypeLine.Contains("Land")) ?? false);
                    if (canPlayLand)
                        actions.Add(new AvailableActionDto { Label = "Jouer un terrain", Type = "PlayLand", Disabled = false });

                    var canPlayCard = (game.Zones[$"{playerId}_hand"]?.Any(c => c.TypeLine == null || !c.TypeLine.Contains("Land")) ?? false);
                    if (canPlayCard)
                        actions.Add(new AvailableActionDto { Label = "Jouer un sort", Type = "PlayCard", Disabled = false });

                    actions.Add(new AvailableActionDto { Label = "Passer à la phase de combat", Type = "PassToCombat", Disabled = false });
                    actions.Add(new AvailableActionDto { Label = "Finir le tour", Type = "EndTurn", Disabled = false });
                    break;

                case Phase.Combat:
                    actions.Add(new AvailableActionDto { Label = "Fin de combat", Type = "ResolveCombat", Disabled = false });
                    break;

                case Phase.End:
                    actions.Add(new AvailableActionDto { Label = "Finir le tour", Type = "EndTurn", Disabled = false });
                    break;
            }

            return actions;
        }

    }
}