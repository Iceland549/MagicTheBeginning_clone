using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace GameMicroservice.Application.UseCases.Combat
{
    public class DeclareAttackersUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<DeclareAttackersUseCase> _logger;
        private readonly IMapper _mapper;

        public DeclareAttackersUseCase(
            IGameRulesEngine engine,
            ILogger<DeclareAttackersUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(
            string sessionId,
            string playerId,
            List<string> attackerIds)
        {
            _logger.LogInformation("[DeclareAttackers] START - Player={PlayerId}, Attackers={Count}",
                playerId, attackerIds?.Count ?? 0);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                // Normalise la liste des attaquants pour éviter tout risque de null
                attackerIds ??= new List<string>();
                if (attackerIds.Count == 0)
                {
                    return new ActionResultDto
                    {
                        Success = false,
                        Message = "Aucune créature déclarée attaquante",
                        GameState = _mapper.Map<GameSessionDto>(session)
                    };
                }

                // Declare attackers
                await _engine.ValidateAttackAsync(session, playerId, attackerIds);
                session = await _engine.DeclareAttackersAsync(session, playerId, attackerIds);
                await _engine.SaveSessionAsync(session);

                session.CurrentPhase = Phase.Combat;
                await _engine.SaveSessionAsync(session);
                _logger.LogInformation("[DeclareAttackers] Phase forcée en Combat après déclaration d’attaquants.");


                var defenderId = session.Players.FirstOrDefault(p => p.PlayerId != playerId)?.PlayerId;

                // Si pas de défenseur identifié -> renvoyer l'état (sécurité)
                if (string.IsNullOrEmpty(defenderId))
                {
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = $"{attackerIds.Count} créatures déclarées attaquantes",
                        GameState = _mapper.Map<GameSessionDto>(session)
                    };
                }

                // Détecter si le défenseur est une IA (même logique que utilisée ailleurs)
                var defenderIsAI = defenderId == "AI" || (session.IsPlayerTwoAI && defenderId == session.PlayerTwoId);

                // Détecter si l'attaquant est une IA (similaire)
                var attackerIsAI = playerId == "AI" || (session.IsPlayerTwoAI && playerId == session.PlayerTwoId);

                if (defenderIsAI)
                {
                    // Récupérer le battlefield du défenseur (liste de CardInGame)
                    var defenderBattlefieldKey = $"{defenderId}_battlefield";

                    List<CardInGame> defenderBattlefield;
                    if (session.Zones != null && session.Zones.ContainsKey(defenderBattlefieldKey))
                    {
                        defenderBattlefield = session.Zones[defenderBattlefieldKey] as List<CardInGame> ?? new List<CardInGame>();
                    }
                    else
                    {
                        defenderBattlefield = new List<CardInGame>();
                    }

                    var defenderCreatureCount = defenderBattlefield.Count;

                    _logger.LogInformation("[DeclareAttackers] Defender {DefenderId} is AI with {Count} creatures", defenderId, defenderCreatureCount);

                    // Si l'IA n'a aucune créature -> auto-resolve
                    if (defenderCreatureCount == 0)
                    {
                        _logger.LogInformation("[DeclareAttackers] Auto-resolve: defender AI has no creatures.");
                        session = await _engine.DeclareBlockersAIAsync(session, defenderId);
                        session = await _engine.ResolveCombatDamageAsync(session, playerId);
                        await _engine.SaveSessionAsync(session);

                        var endGame = _engine.CheckEndGame(session);
                        return new ActionResultDto
                        {
                            Success = true,
                            Message = $"{attackerIds.Count} attaquants résolus automatiquement (IA sans bloqueurs)",
                            GameState = _mapper.Map<GameSessionDto>(session),
                            EndGame = endGame
                        };
                    }

                    // Si IA a des créatures -> l'IA déclare ses bloqueurs
                    session = await _engine.DeclareBlockersAIAsync(session, defenderId);
                    await _engine.SaveSessionAsync(session);

                    // Si les deux joueurs sont des IA (attaquant IA), résoudre automatiquement
                    if (attackerIsAI)
                    {
                        _logger.LogInformation("[DeclareAttackers] Both players are AI -> auto-resolving after blockers.");
                        session = await _engine.ResolveCombatDamageAsync(session, playerId);
                        await _engine.SaveSessionAsync(session);
                        var endGame = _engine.CheckEndGame(session);

                        return new ActionResultDto
                        {
                            Success = true,
                            Message = $"{attackerIds.Count} attaquants résolus automatiquement (IA vs IA)",
                            GameState = _mapper.Map<GameSessionDto>(session),
                            EndGame = endGame
                        };
                    }

                    // Sinon (attaquant humain vs IA) : retourne l'état après que l'IA ait déclaré ses bloqueurs
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = $"{attackerIds.Count} créatures déclarées attaquantes (IA a déclaré ses bloqueurs)",
                        GameState = _mapper.Map<GameSessionDto>(session)
                    };
                }
                else
                {
                    // Défenseur humain : le front du défenseur doit appeler DeclareBlockers
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = $"{attackerIds.Count} créatures déclarées attaquantes",
                        GameState = _mapper.Map<GameSessionDto>(session)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeclareAttackers] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }


    }
}
