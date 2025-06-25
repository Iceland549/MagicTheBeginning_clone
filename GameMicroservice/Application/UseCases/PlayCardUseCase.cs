using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;

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

        public async Task<GameSessionDto?> ExecuteAsync(string sessionId, string playerId, PlayerActionDto action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var sessionEntity = await _repo.GetByIdAsync(sessionId);
            if (sessionEntity == null)
                return null;

            if (!_engine.HasDrawnThisTurn(sessionEntity, playerId))
                sessionEntity = _engine.DrawStep(sessionEntity, playerId);

            switch (action.Type)
            {
                case ActionType.PlayLand:
                    if (_engine.IsLandPhase(sessionEntity, playerId))
                    {
                        if (string.IsNullOrEmpty(action.CardId))
                            throw new InvalidOperationException("CardId is required for PlayLand action");

                        await _engine.ValidatePlayLandAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = _engine.PlayLand(sessionEntity, playerId, action.CardId);
                        sessionEntity = _engine.OnLandfall(sessionEntity, playerId, action.CardId);
                    }
                    break;

                case ActionType.PlayCard:
                    if (_engine.IsMainPhase(sessionEntity, playerId))
                    {
                        if (string.IsNullOrEmpty(action.CardId))
                            throw new InvalidOperationException("CardId is required for PlayCard action");

                        await _engine.ValidatePlayAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = await _engine.PlayCardAsync(sessionEntity, playerId, action.CardId);
                    }
                    break;

                case ActionType.CastInstant:
                    if (_engine.IsSpellPhase(sessionEntity, playerId))
                    {
                        if (string.IsNullOrEmpty(action.CardId))
                            throw new InvalidOperationException("CardId is required for CastInstant action");

                        await _engine.ValidateInstantAsync(sessionEntity, playerId, action.CardId);
                        sessionEntity = await _engine.CastInstantAsync(sessionEntity, playerId, action.CardId, action.TargetId);
                    }
                    break;

                case ActionType.PassToCombat:
                    if (_engine.IsSpellPhase(sessionEntity, playerId))
                        sessionEntity = _engine.StartCombatPhase(sessionEntity, playerId);
                    break;

                case ActionType.Attack:
                    if (_engine.IsCombatPhase(sessionEntity, playerId))
                    {
                        if (action.Attackers == null || !action.Attackers.Any())
                            throw new InvalidOperationException("Attackers list is required for Attack action");
                        if (action.Blockers == null)
                            throw new InvalidOperationException("Blockers dictionary is required for Attack action");

                        await _engine.ValidateAttackAsync(sessionEntity, playerId, action.Attackers);
                        sessionEntity = await _engine.ResolveCombatAsync(sessionEntity, playerId, action.Attackers, action.Blockers);
                    }
                    break;

                case ActionType.PreEnd:
                    if (_engine.IsPreEndPhase(sessionEntity, playerId))
                        sessionEntity = _engine.PreEndCheck(sessionEntity, playerId);
                    break;

                case ActionType.EndTurn:
                    if (_engine.IsEndPhase(sessionEntity, playerId))
                        sessionEntity = _engine.EndTurn(sessionEntity, playerId);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid action type: {action.Type}");
            }

            await _repo.UpdateAsync(sessionEntity);
            return _mapper.Map<GameSessionDto>(sessionEntity);
        }
    }
}