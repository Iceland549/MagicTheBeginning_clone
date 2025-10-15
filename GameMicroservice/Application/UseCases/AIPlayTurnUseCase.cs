using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
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
            // Load current session (entity)
            var sessionEntity = await _engine.LoadSessionAsync(sessionId);
            if (sessionEntity == null)
                return null;

            // Draw step if needed
            if (!_engine.HasDrawnThisTurn(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.DrawStep(sessionEntity, sessionEntity.ActivePlayerId);

            // AI decision loop
            while (true)
            {
                var state = sessionEntity.Players.FirstOrDefault(p => p.PlayerId == sessionEntity.ActivePlayerId)
                    ?? throw new InvalidOperationException($"Player {sessionEntity.ActivePlayerId} not found");

                var handKey = $"{sessionEntity.ActivePlayerId}_hand";
                var hand = sessionEntity.Zones.ContainsKey(handKey)
                    ? sessionEntity.Zones[handKey]
                    : new List<CardInGame>();

                var action = _ai.DecideNextAction(state, sessionEntity, hand);

                // If AI returns null or explicit EndTurn -> switch to End phase and break
                if (action == null || action.Type == ActionType.EndTurn)
                {
                    Console.WriteLine("[AIPlayTurn] AI ends turn (null or EndTurn). Forcing End phase and persisting.");
                    sessionEntity.CurrentPhase = Phase.End;
                    await _engine.SaveSessionAsync(sessionEntity);
                    break;
                }

                // Handle PlayLand
                if (action.Type == ActionType.PlayLand && !string.IsNullOrEmpty(action.CardId))
                {
                    try
                    {
                        // Optional: validate (will throw if invalid)
                        await _engine.ValidatePlayLandAsync(sessionEntity, sessionEntity.ActivePlayerId, action.CardId);

                        // Apply the PlayLand mutation (synchronous)
                        sessionEntity = _engine.PlayLand(sessionEntity, sessionEntity.ActivePlayerId, action.CardId);

                        // Persist changes
                        await _engine.SaveSessionAsync(sessionEntity);
                        Console.WriteLine($"[AIPlayTurn] Played land {action.CardId}");

                        // Refresh hand reference since zones mutated
                        hand = sessionEntity.Zones.ContainsKey(handKey) ? sessionEntity.Zones[handKey] : new List<CardInGame>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AIPlayTurn] Impossible de jouer le land {action.CardId}: {ex.Message}");
                        // Defensive: remove the problematic card from hand so AI won't loop on it
                        if (hand != null)
                            hand.RemoveAll(c => c.CardId == action.CardId);
                    }
                    continue; // next decision iteration
                }

                // Handle PlayCard: delegate to PlayCardUseCase (which returns ActionResultDto with GameState = GameSessionDto)
                if (action.Type == ActionType.PlayCard && !string.IsNullOrEmpty(action.CardId))
                {
                    var result = await _play.ExecuteAsync(sessionId, sessionEntity.ActivePlayerId, action);

                    if (result == null || !result.Success || result.GameState == null)
                    {
                        Console.WriteLine($"[AIPlayTurn] Impossible de jouer {action.CardId}, on essaie autre chose");
                        // Remove this card from hand to avoid infinite loop on impossible-to-play card
                        if (hand != null)
                            hand.RemoveAll(c => c.CardId == action.CardId);
                        continue;
                    }

                    // Map returned DTO -> entity so further iterations operate on an updated entity
                    sessionEntity = _mapper.Map<GameSession>(result.GameState);
                }
            }

            // After decision loop, resolve combat / pre-end if needed
            if (_engine.IsCombatPhase(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.ResolveCombatPhase(sessionEntity, sessionEntity.ActivePlayerId);

            if (_engine.IsPreEndPhase(sessionEntity, sessionEntity.ActivePlayerId))
                sessionEntity = _engine.PreEndCheck(sessionEntity, sessionEntity.ActivePlayerId);

            // Ensure End phase before calling EndTurn (EndTurn requires IsEndPhase)
            if (!_engine.IsEndPhase(sessionEntity, sessionEntity.ActivePlayerId))
            {
                sessionEntity.CurrentPhase = Phase.End;
                await _engine.SaveSessionAsync(sessionEntity);
                Console.WriteLine("[AIPlayTurn] Forced End phase before EndTurn call.");
            }

            // Perform EndTurn (engine will swap active player, cleanup, reset lands played, etc.)
            sessionEntity = _engine.EndTurn(sessionEntity, sessionEntity.ActivePlayerId);

            // Persist final session
            await _engine.SaveSessionAsync(sessionEntity);

            // Return DTO (mapped via AutoMapper profile already present in project)
            return _mapper.Map<GameSessionDto>(sessionEntity);
        }
    }
}
