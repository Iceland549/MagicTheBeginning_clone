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
            _engine = engine;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActionResultDto?> ExecuteAsync(string sessionId, string playerId)
        {
            _logger.LogInformation("===== [PlayerPlayTurn] START ===== SessionId={SessionId}, PlayerId={PlayerId}", sessionId, playerId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session not found." };

            var player = session.Players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
                return new ActionResultDto { Success = false, Message = "Player not found." };

            session.CurrentPhase = Phase.Draw;
            await _engine.SaveSessionAsync(session);
            await Task.Delay(3000);

            try
            {
                // --- 🟩 Combat Phase ---
                if (_engine.IsCombatPhase(session, playerId))
                {
                    _logger.LogInformation("[PlayerPlayTurn] Combat phase started.");

                    // 1️⃣ Start combat phase
                    session = await _engine.StartCombatPhaseAsync(session, playerId);

                    // 2️⃣ Déclaration des attaquants (depuis frontend)
                    var declaredAttackers = session.Zones.ContainsKey("_declared_attackers")
                        ? session.Zones["_declared_attackers"].Select(c => c.CardId).ToList()
                        : new List<string>();
                    if (declaredAttackers.Any())
                    {
                        session = await _engine.DeclareAttackersAsync(session, playerId, declaredAttackers);
                        _logger.LogInformation("[PlayerPlayTurn] {Count} attackers declared.", declaredAttackers.Count);
                    }

                    // 3️⃣ L’IA bloque
                    var defender = session.Players.FirstOrDefault(p => p.PlayerId != playerId);
                    if (defender != null)
                    {
                        session = await _engine.DeclareBlockersAIAsync(session, defender.PlayerId);
                        _logger.LogInformation("[PlayerPlayTurn] AI blockers declared.");
                    }

                    // 4️⃣ Résolution des dégâts
                    session = await _engine.ResolveCombatDamageAsync(session, playerId);
                    _logger.LogInformation("[PlayerPlayTurn] Combat resolved.");
                }

                // --- 🟨 Pre-End Checks ---
                if (_engine.IsPreEndPhase(session, playerId))
                {
                    _logger.LogInformation("[PlayerPlayTurn] Executing pre-end checks.");
                    session = _engine.PreEndCheck(session, playerId);
                }

                // --- 🟥 End Phase ---
                if (!_engine.IsEndPhase(session, playerId))
                    session.CurrentPhase = Phase.End;

                session = _engine.EndTurn(session, playerId);

                // --- 🧹 Gestion de la main (discard si > 7 cartes) ---
                var handKey = $"{playerId}_hand";
                if (session.Zones.ContainsKey(handKey) && session.Zones[handKey].Count > 7)
                {
                    var excess = session.Zones[handKey].Count - 7;
                    var toDiscard = session.Zones[handKey]
                        .OrderByDescending(c => ManaCostHelper.ComputeTotalManaValue(c.ManaCost ?? ""))
                        .Take(excess)
                        .Select(c => c.CardId)
                        .ToList();

                    session = await _engine.DiscardCards(session, playerId, toDiscard, new());
                    _logger.LogInformation("[PlayerPlayTurn] Discarded {Excess} cards.", excess);
                }

                await _engine.SaveSessionAsync(session);

                var endGame = _engine.CheckEndGame(session);
                if (endGame != null)
                    _logger.LogInformation("[PlayerPlayTurn] GAME ENDED — Winner={Winner}, Reason={Reason}", endGame.WinnerId, endGame.Reason);

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
                _logger.LogError(ex, "[PlayerPlayTurn] Error during turn end for {PlayerId}.", playerId);
                throw;
            }
        }
    }

}