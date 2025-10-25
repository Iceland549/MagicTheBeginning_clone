using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Helpers;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Human player turn orchestrator.
    /// Handles ONLY the end-of-turn logic.
    /// Card playing is done via PlayLandUseCase and PlayCardUseCase.
    /// </summary>
    public class PlayerPlayTurnUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;
        private readonly ILogger<PlayerPlayTurnUseCase> _logger;

        public PlayerPlayTurnUseCase(
            IGameRulesEngine engine,
            IMapper mapper,
            ILogger<PlayerPlayTurnUseCase> logger)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ActionResultDto?> ExecuteAsync(string sessionId, string playerId)
        {
            _logger.LogInformation("===== [PlayerPlayTurn] START ===== SessionId={SessionId}, PlayerId={PlayerId}", sessionId, playerId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("[PlayerPlayTurn] Session {SessionId} not found.", sessionId);
                return new ActionResultDto { Success = false, Message = "Session not found." };
            }

            var player = session.Players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                _logger.LogWarning("[PlayerPlayTurn] Player {PlayerId} not found in session.", playerId);
                return new ActionResultDto { Success = false, Message = "Player not found." };
            }

            _logger.LogDebug("[PlayerPlayTurn] State: Phase={Phase}, LandsPlayed={Lands}, Drawn={Drawn}",
                session.CurrentPhase, player.LandsPlayedThisTurn, player.HasDrawnThisTurn);

            session.CurrentPhase = Phase.Draw;

            await _engine.SaveSessionAsync(session); 

            _logger.LogInformation("[PlayerPlayTurn] Displaying 'À toi de jouer' animation...");
            await Task.Delay(3000);

            try
            {
                if (_engine.IsCombatPhase(session, playerId))
                {
                    _logger.LogInformation("[PlayerPlayTurn] Resolving combat phase.");
                    session = _engine.ResolveCombatPhase(session, playerId);
                }

                if (_engine.IsPreEndPhase(session, playerId))
                {
                    _logger.LogInformation("[PlayerPlayTurn] Executing pre-end checks.");
                    session = _engine.PreEndCheck(session, playerId);
                }

                if (!_engine.IsEndPhase(session, playerId))
                {
                    _logger.LogDebug("[PlayerPlayTurn] Forcing End phase.");
                    session.CurrentPhase = Phase.End;
                }

                session = _engine.EndTurn(session, playerId);
                _logger.LogInformation("[PlayerPlayTurn] EndTurn executed. Next player={NextPlayer}.", session.ActivePlayerId);

                // Discard step
                var handKey = $"{playerId}_hand";
                if (session.Zones.ContainsKey(handKey) && session.Zones[handKey].Count > 7)
                {
                    var excess = session.Zones[handKey].Count - 7;
                    var toDiscard = session.Zones[handKey]
                        .OrderByDescending(c => ManaCostHelper.ComputeTotalManaValue(c.ManaCost ?? ""))
                        .Take(excess)
                        .Select(c => c.CardId)
                        .ToList();

                    _logger.LogInformation("[PlayerPlayTurn] Player {PlayerId} must discard {Excess} cards: {Cards}",
                        playerId, excess, string.Join(", ", toDiscard));

                    session = await _engine.DiscardCards(session, playerId, toDiscard, new());
                }

                await _engine.SaveSessionAsync(session);
                _logger.LogInformation("[PlayerPlayTurn] Session saved successfully.");

                var endGame = _engine.CheckEndGame(session);
                if (endGame != null)
                {
                    _logger.LogInformation("[PlayerPlayTurn] GAME ENDED — Winner={Winner}, Reason={Reason}",
                        endGame.WinnerId, endGame.Reason);
                }

                var handCount = session.Zones.ContainsKey(handKey) ? session.Zones[handKey].Count : 0;
                var fieldCount = session.Zones.ContainsKey($"{playerId}_battlefield") ? session.Zones[$"{playerId}_battlefield"].Count : 0;
                var manaDump = string.Join(", ", player.ManaPool.Select(kv => $"{kv.Key}:{kv.Value}"));

                _logger.LogInformation("[PlayerPlayTurn][StateDump] => Phase={Phase}, Hand={Hand}, Battlefield={Field}, Mana=[{Mana}]",
                    session.CurrentPhase, handCount, fieldCount, manaDump);

                _logger.LogInformation("===== [PlayerPlayTurn] END =====");

                return new ActionResultDto
                {
                    Success = true,
                    Message = "Turn completed successfully.",
                    GameState = _mapper.Map<GameSessionDto>(session),
                    EndGame = endGame
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PlayerPlayTurn] Exception during turn end for {PlayerId}.", playerId);
                throw;
            }
        }
    }
}