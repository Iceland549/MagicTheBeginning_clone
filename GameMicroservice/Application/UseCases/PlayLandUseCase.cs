using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Use case: the player plays a land card (from hand to battlefield).
    /// Enforces one-land-per-turn and logs all important actions.
    /// </summary>
    public class PlayLandUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;
        private readonly ILogger<PlayLandUseCase> _logger;

        public PlayLandUseCase(
            IGameSessionRepository repo,
            IGameRulesEngine engine,
            IMapper mapper,
            ILogger<PlayLandUseCase> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, string cardId)
            => await ExecuteAsync(sessionId, playerId, cardId, null);

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, string cardId, string? chosenColor)
        {
            _logger.LogInformation("[PlayLandUseCase] START — SessionId={SessionId}, PlayerId={PlayerId}, CardId={CardId}", sessionId, playerId, cardId);

            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("[PlayLandUseCase] Session {SessionId} introuvable.", sessionId);
                return new ActionResultDto { Success = false, Message = "Session introuvable" };
            }

            if (!_engine.IsLandPhase(session, playerId))
            {
                _logger.LogWarning("[PlayLandUseCase] Pas la phase de terrain pour {PlayerId}. Phase actuelle={Phase}", playerId, session.CurrentPhase);
                return new ActionResultDto { Success = false, Message = "Pas la phase de terrain" };
            }

            try
            {
                // ✅ Validation logique métier
                _logger.LogDebug("[PlayLandUseCase] Validation du terrain {CardId}…", cardId);
                await _engine.ValidatePlayLandAsync(session, playerId, cardId);
                _logger.LogInformation("[PlayLandUseCase] Validation réussie pour {CardId}.", cardId);

                // ✅ Exécution de l’action
                session = _engine.PlayLand(session, playerId, cardId);
                _logger.LogInformation("[PlayLandUseCase] Terrain {CardId} joué avec succès. LandsPlayedThisTurn={Count}",
                    cardId,
                    session.Players.First(p => p.PlayerId == playerId).LandsPlayedThisTurn);

                // 💾 Sauvegarde
                await _repo.UpdateAsync(session);
                _logger.LogInformation("[PlayLandUseCase] Session sauvegardée après jeu de terrain.");

                return new ActionResultDto
                {
                    Success = true,
                    Message = $"Terrain {cardId} joué avec succès.",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }
            catch (InvalidOperationException ex)
            {
                // ⚠️ Cas logique (ex. : deuxième terrain)
                _logger.LogWarning(ex, "[PlayLandUseCase] Échec logique lors du jeu du terrain {CardId}: {Message}", cardId, ex.Message);
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                // ❌ Cas technique (erreur inattendue)
                _logger.LogError(ex, "[PlayLandUseCase] Erreur inattendue lors du jeu du terrain {CardId}.", cardId);
                return new ActionResultDto { Success = false, Message = "Erreur interne lors du jeu du terrain" };
            }
        }
    }
}
