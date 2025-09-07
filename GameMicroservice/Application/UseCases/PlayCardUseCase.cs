using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;

namespace GameMicroservice.Application.UseCases
{
    public class PlayCardUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public PlayCardUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, PlayerActionDto action)
        {
            if (action == null)
                return new ActionResultDto { Success = false, Message = "Action manquante" };

            var sessionEntity = await _repo.GetByIdAsync(sessionId);
            if (sessionEntity == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                switch (action.Type)
                {
                    case ActionType.PlayLand:
                        if (!_engine.IsLandPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de terrain" };
                        if (string.IsNullOrEmpty(action.CardId))
                            return new ActionResultDto { Success = false, Message = "CardId requis pour PlayLand" };

                        await _engine.ValidatePlayLandAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = _engine.PlayLand(sessionEntity, playerId, action.CardId);
                        sessionEntity = _engine.OnLandfall(sessionEntity, playerId, action.CardId);
                        break;

                    case ActionType.PlayCard:
                        if (!_engine.IsMainPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase principale" };
                        if (string.IsNullOrEmpty(action.CardId))
                            return new ActionResultDto { Success = false, Message = "CardId requis pour PlayCard" };

                        await _engine.ValidatePlayAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = await _engine.PlayCardAsync(sessionEntity, playerId, action.CardId);
                        break;

                    case ActionType.CastInstant:
                        if (!_engine.IsSpellPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de sorts" };
                        if (string.IsNullOrEmpty(action.CardId))
                            return new ActionResultDto { Success = false, Message = "CardId requis pour CastInstant" };

                        await _engine.ValidateInstantAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = await _engine.CastInstantAsync(sessionEntity, playerId, action.CardId, action.TargetId);
                        break;

                    case ActionType.PassToCombat:
                        if (!_engine.IsSpellPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Impossible de passer à la phase de combat" };
                        sessionEntity = _engine.StartCombatPhase(sessionEntity, playerId);
                        break;

                    case ActionType.Attack:
                        if (!_engine.IsCombatPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de combat" };
                        if (action.Attackers == null || !action.Attackers.Any())
                            return new ActionResultDto { Success = false, Message = "Liste des attaquants manquante" };
                        if (action.Blockers == null)
                            return new ActionResultDto { Success = false, Message = "Dictionnaire des bloqueurs manquant" };

                        await _engine.ValidateAttackAsync(sessionEntity, playerId, action.Attackers);
                        sessionEntity = await _engine.ResolveCombatAsync(sessionEntity, playerId, action.Attackers, action.Blockers);
                        break;

                    case ActionType.Block:
                        if (!_engine.IsBlockPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de blocage" };
                        if (action.Blockers == null || !action.Blockers.Any())
                            return new ActionResultDto { Success = false, Message = "Aucun bloqueur fourni" };

                        await _engine.ValidateBlockAsync(sessionEntity, playerId, action.Blockers);
                        sessionEntity = await _engine.ResolveBlockAsync(sessionEntity, playerId, action.Blockers);
                        break;

                    case ActionType.Discard:
                        if (action.CardsToDiscard == null || !action.CardsToDiscard.Any())
                            return new ActionResultDto { Success = false, Message = "Aucune carte à défausser" };
                        sessionEntity = await _engine.DiscardCards(sessionEntity, playerId, action.CardsToDiscard, new Dictionary<string, string>());
                        break;

                    case ActionType.PreEnd:
                        if (!_engine.IsPreEndPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de pré-fin" };
                        sessionEntity = _engine.PreEndCheck(sessionEntity, playerId);
                        break;

                    case ActionType.EndTurn:
                        if (!_engine.IsEndPhase(sessionEntity, playerId))
                            return new ActionResultDto { Success = false, Message = "Pas la phase de fin de tour" };
                        sessionEntity = _engine.EndTurn(sessionEntity, playerId);
                        break;

                    case ActionType.EndGame:
                        break;

                    default:
                        return new ActionResultDto { Success = false, Message = $"Action inconnue : {action.Type}" };
                }

                // Vérification de la fin de partie
                var endGame = _engine.CheckEndGame(sessionEntity);
                await _repo.UpdateAsync(sessionEntity);

                return new ActionResultDto
                {
                    Success = true,
                    Message = "Carte jouée",
                    GameState = _mapper.Map<GameSessionDto>(sessionEntity),
                    EndGame = endGame
                };
            }
            catch (Exception ex)
            {
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}
