using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases.Combat
{
    public class ResolveCombatUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<ResolveCombatUseCase> _logger;
        private readonly IMapper _mapper;

        public ResolveCombatUseCase(
            IGameRulesEngine engine,
            ILogger<ResolveCombatUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId)
        {
            _logger.LogInformation("[ResolveCombat] START - Session={SessionId}, Player={PlayerId}", sessionId, playerId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                // --- Étape 1 : Résoudre les dégâts du combat
                session = await _engine.ResolveCombatDamageAsync(session, playerId);
                await _engine.SaveSessionAsync(session);

                _logger.LogInformation("[ResolveCombat] Dégâts de combat résolus avec succès");

                // --- Étape 2 : Vérifier si la partie est terminée
                var endGame = _engine.CheckEndGame(session);
                if (endGame != null)
                {
                    _logger.LogInformation("[ResolveCombat] Fin de partie détectée - Winner={Winner}", endGame.WinnerId);
                    return new ActionResultDto
                    {
                        Success = true,
                        Message = "Partie terminée après résolution du combat",
                        GameState = _mapper.Map<GameSessionDto>(session),
                        EndGame = endGame
                    };
                }

                // --- Étape 3 : Identifier si l'attaquant était une IA
                var attacker = session.Players.FirstOrDefault(p => p.PlayerId != playerId);
                var attackerIsAI = attacker != null &&
                                   (attacker.PlayerId == "AI" ||
                                   (session.IsPlayerTwoAI && attacker.PlayerId == session.PlayerTwoId));

                if (attackerIsAI)
                {
                    _logger.LogInformation("[ResolveCombat] Attaquant IA détecté → Fin automatique du tour IA");

                    if (attacker != null)
                    {
                        _logger.LogInformation("[ResolveCombat] Fin du tour IA (attaquant={AttackerId})", attacker.PlayerId);

                        // 🔹 Forcer proprement la phase de fin avant d'appeler EndTurn
                        _logger.LogInformation("[ResolveCombat] Forçage explicite de la phase 'End' avant EndTurn()");

                        // 1️⃣ Forcer la phase dans la session
                        session.CurrentPhase = Phase.End;

                        // 2️⃣ Sauvegarder avant d’appeler EndTurn (pour que le moteur relise la bonne phase)
                        await _engine.SaveSessionAsync(session);

                        // 3️⃣ Appel sécurisé de EndTurn
                        try
                        {
                            session = _engine.EndTurn(session, attacker.PlayerId);
                            _logger.LogInformation("[ResolveCombat] ✅ EndTurn exécuté avec succès pour {AttackerId}", attacker.PlayerId);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogWarning(ex, "[ResolveCombat] Phase non reconnue comme 'End' par le moteur, correction forcée.");
                            session.CurrentPhase = Phase.End;
                            await _engine.SaveSessionAsync(session);
                            session = _engine.EndTurn(session, attacker.PlayerId);
                        }



                        // 🔸 Repasser au joueur humain
                        var nextPlayer = session.Players.FirstOrDefault(p => p.PlayerId != attacker.PlayerId);
                        if (nextPlayer != null)
                        {
                            session.ActivePlayerId = nextPlayer.PlayerId;
                            _logger.LogInformation("[ResolveCombat] Le tour est désormais au joueur humain ({NextPlayer})", nextPlayer.PlayerId);
                        }
                    }
                    else
                    {
                        // 🔹 Sécurité : aucun attaquant identifiable
                        _logger.LogWarning("[ResolveCombat] Aucun attaquant trouvé, passage direct à la phase End.");
                        session.CurrentPhase = Phase.End;
                    }

                    await _engine.SaveSessionAsync(session);
                }


                // --- Étape 4 : Retourner l'état du jeu actualisé
                _logger.LogInformation("[ResolveCombat] SUCCESS - Phase={Phase}, ActivePlayer={ActivePlayer}",
                session.CurrentPhase, session.ActivePlayerId);

                return new ActionResultDto
                {
                    Success = true,
                    Message = "Combat résolu et tour terminé",
                    GameState = _mapper.Map<GameSessionDto>(session),
                    EndGame = _engine.CheckEndGame(session)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ResolveCombat] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}
