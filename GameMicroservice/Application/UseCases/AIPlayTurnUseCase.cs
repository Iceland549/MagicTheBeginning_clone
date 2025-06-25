using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class AIPlayTurnUseCase
    {
        private readonly PlayCardUseCase _play;
        private readonly IGameRulesEngine _engine;
        private readonly IAIEngine _ai;
        private readonly IMapper _mapper;

        public AIPlayTurnUseCase(
            PlayCardUseCase play,
            IGameRulesEngine engine,
            IAIEngine ai,
            IMapper mapper)
        {
            _play = play ?? throw new ArgumentNullException(nameof(play));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _ai = ai ?? throw new ArgumentNullException(nameof(ai));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<GameSessionDto?> ExecuteAsync(string sessionId)
        {
            // Load current session
            var sessionEntity = await _engine.LoadSessionAsync(sessionId);
            if (sessionEntity == null)
                return null;

            // Draw step
            if (!_engine.HasDrawnThisTurn(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.DrawStep(sessionEntity, sessionEntity.ActivePlayerId);

            // AI decision loop
            while (true)
            {
                var state = sessionEntity.Players.FirstOrDefault(p => p.PlayerId == sessionEntity.ActivePlayerId)
                    ?? throw new InvalidOperationException($"Player {sessionEntity.ActivePlayerId} not found");
                var handKey = $"{sessionEntity.ActivePlayerId}_hand";
                var hand = sessionEntity.Zones.ContainsKey(handKey) ? sessionEntity.Zones[handKey] : new List<CardInGame>();
                var action = _ai.DecideNextAction(state, sessionEntity, hand);
                if (action == null || action.Type == ActionType.EndTurn)
                    break;

                // Execute action via PlayCardUseCase
                if (action.Type == ActionType.PlayCard && !string.IsNullOrEmpty(action.CardId))
                {
                    var sessionDto = await _play.ExecuteAsync(sessionId, sessionEntity.ActivePlayerId, action);
                    if (sessionDto == null)
                        return null;
                    sessionEntity = _mapper.Map<GameSession>(sessionDto);
                }
            }

            // Ensure combat/pre-end
            if (_engine.IsCombatPhase(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.ResolveCombatPhase(sessionEntity, sessionEntity.ActivePlayerId);
            if (_engine.IsPreEndPhase(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.PreEndCheck(sessionEntity, sessionEntity.ActivePlayerId);

            // End turn
            sessionEntity = _engine.EndTurn(sessionEntity, sessionEntity.ActivePlayerId);

            // Persist and return
            await _engine.SaveSessionAsync(sessionEntity);
            return _mapper.Map<GameSessionDto>(sessionEntity);
        }
    }
}