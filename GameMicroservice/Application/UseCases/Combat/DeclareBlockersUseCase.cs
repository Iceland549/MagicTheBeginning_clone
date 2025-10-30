using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases.Combat
{
    public class DeclareBlockersUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<DeclareBlockersUseCase> _logger;
        private readonly IMapper _mapper;

        public DeclareBlockersUseCase(
            IGameRulesEngine engine,
            ILogger<DeclareBlockersUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(
            string sessionId,
            string playerId,
            Dictionary<string, string> blockers)
        {
            _logger.LogInformation("[DeclareBlockers] START - Player={PlayerId}, Blockers={Count}",
                playerId, blockers?.Count ?? 0);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                // Étape 1 - Déclaration des bloqueurs
                if (playerId == "AI" || playerId == session.PlayerTwoId)
                {
                    session = await _engine.DeclareBlockersAIAsync(session, playerId);
                }
                else
                {
                    session = await _engine.DeclareBlockersAsync(session, playerId, blockers ?? new Dictionary<string, string>());
                }
                await _engine.SaveSessionAsync(session);

                // Étape 2 - Identifier l’attaquant
                var attacker = session.Players.FirstOrDefault(p => p.PlayerId != playerId);
                if (attacker == null)
                    return new ActionResultDto { Success = false, Message = "Attaquant introuvable" };

                var attackerIsAI = attacker.PlayerId == "AI" || (session.IsPlayerTwoAI && attacker.PlayerId == session.PlayerTwoId);
                var defenderIsAI = playerId == "AI" || (session.IsPlayerTwoAI && playerId == session.PlayerTwoId);

                // Étape 3 - Si l’attaquant est IA → auto resolve combat
                if (attackerIsAI && !defenderIsAI)
                {
                    _logger.LogInformation("[DeclareBlockers] Attaquant IA détecté → résolution auto du combat");
                    session = await _engine.ResolveCombatDamageAsync(session, attacker.PlayerId);
                    await _engine.SaveSessionAsync(session);

                    // Fin de tour IA
                    if (_engine.IsPreEndPhase(session, attacker.PlayerId))
                        session = _engine.PreEndCheck(session, attacker.PlayerId);

                    if (!_engine.IsEndPhase(session, attacker.PlayerId))
                        session.CurrentPhase = Phase.End;

                    session = _engine.EndTurn(session, attacker.PlayerId);
                    await _engine.SaveSessionAsync(session);

                    // Changer le joueur actif (repasser au joueur humain)
                    var nextPlayer = session.Players.FirstOrDefault(p => p.PlayerId != attacker.PlayerId);
                    if (nextPlayer != null)
                        session.ActivePlayerId = nextPlayer.PlayerId;

                    var endGame = _engine.CheckEndGame(session);
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = "Combat résolu automatiquement (IA attaquante)",
                        GameState = _mapper.Map<GameSessionDto>(session),
                        EndGame = endGame
                    };
                }

                // Étape 4 - Si IA vs IA → auto resolve aussi
                if (attackerIsAI && defenderIsAI)
                {
                    _logger.LogInformation("[DeclareBlockers] IA vs IA → résolution automatique complète");
                    session = await _engine.ResolveCombatDamageAsync(session, attacker.PlayerId);
                    session.CurrentPhase = Phase.End;
                    session = _engine.EndTurn(session, attacker.PlayerId);
                    await _engine.SaveSessionAsync(session);

                    var endGame = _engine.CheckEndGame(session);
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = "Combat IA vs IA résolu",
                        GameState = _mapper.Map<GameSessionDto>(session),
                        EndGame = endGame
                    };
                }

                // Étape 5 - Sinon (humain attaque) → juste retour d’état
                return new ActionResultDto
                {
                    Success = true,
                    Message = $"{blockers?.Count ?? 0} bloqueurs déclarés",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeclareBlockers] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}
