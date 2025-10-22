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
using System.Threading;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// AI turn orchestrator — drives the AI player's actions using GameRulesEngine and RandomAIEngine.
    /// </summary>
    public class AIPlayTurnUseCase
    {
        private readonly ILogger<AIPlayTurnUseCase> _logger;
        private readonly IGameRulesEngine _engine;
        private readonly IAIEngine _ai;
        private readonly IMapper _mapper;

        public AIPlayTurnUseCase(
            ILogger<AIPlayTurnUseCase> logger,
            IGameRulesEngine engine,
            IAIEngine ai,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _ai = ai ?? throw new ArgumentNullException(nameof(ai));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<GameSessionDto?> ExecuteAsync(string sessionId)
        {
            _logger.LogInformation("[AIPlayTurn] START — SessionId={SessionId}", sessionId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("[AIPlayTurn] Session {SessionId} not found.", sessionId);
                return null;
            }

            var aiId = session.ActivePlayerId;
            _logger.LogInformation("[AIPlayTurn] Active player is AI ({AIId})", aiId);

            // 🟩 Draw step
            if (!_engine.HasDrawnThisTurn(session, aiId))
            {
                session = await _engine.DrawStepAsync(session, aiId);
                await _engine.SaveSessionAsync(session);
                _logger.LogInformation("[AIPlayTurn] AI drew a card at the start of the turn.");
            }

            var sem = new SemaphoreSlim(1, 1);
            var rng = new Random();

            int iterations = 0;
            const int MAX_ITER = 20;

            while (iterations++ < MAX_ITER)
            {
                // 💤 Simulated "thinking" delay
                await sem.WaitAsync();
                try
                {
                    int delay = rng.Next(700, 1300);
                    await Task.Delay(delay);
                }
                finally
                {
                    sem.Release();
                }

                // Refresh latest session and AI state
                session = await _engine.LoadSessionAsync(sessionId);
                var aiState = session.Players.First(p => p.PlayerId == aiId);
                var handKey = $"{aiId}_hand";
                var hand = session.Zones.ContainsKey(handKey) ? session.Zones[handKey] : new List<CardInGame>();

                // Let AI pick next action
                var action = _ai.DecideNextAction(aiState, session, hand);
                if (action == null || action.Type == ActionType.EndTurn)
                {
                    _logger.LogInformation("[AIPlayTurn] AI decided to end turn.");
                    break; // ✅ Only break when AI explicitly wants to end
                }

                try
                {
                    switch (action.Type)
                    {
                        case ActionType.PlayLand:
                            if (string.IsNullOrEmpty(action.CardId))
                            {
                                _logger.LogWarning("[AIPlayTurn] Skipping PlayLand — CardId is null.");
                                continue;
                            }

                            await _engine.ValidatePlayLandAsync(session, aiId, action.CardId);
                            session = _engine.PlayLand(session, aiId, action.CardId);
                            await _engine.SaveSessionAsync(session);

                            session = await _engine.LoadSessionAsync(sessionId);
                            _logger.LogInformation("[AIPlayTurn] Played land {CardId}.", action.CardId);
                            break;

                        case ActionType.PlayCard:
                            if (string.IsNullOrEmpty(action.CardId))
                            {
                                _logger.LogWarning("[AIPlayTurn] Skipping PlayCard — CardId is null.");
                                continue;
                            }

                            // refresh session
                            session = await _engine.LoadSessionAsync(sessionId);

                            // find the card in AI's hand
                            var handKeyLocal = $"{aiId}_hand";
                            var cardInHand = session.Zones.ContainsKey(handKeyLocal)
                                ? session.Zones[handKeyLocal].FirstOrDefault(c => c.CardId == action.CardId)
                                : null;

                            if (cardInHand == null)
                            {
                                _logger.LogWarning("[AIPlayTurn] Card {CardId} not found in hand (maybe already played).", action.CardId);
                                continue;
                            }

                            // compute mana required (total)
                            int manaNeeded = ManaCostHelper.ComputeTotalManaValue(cardInHand.ManaCost ?? "");

                            // compute current mana pool available
                            var playerState = session.Players.First(p => p.PlayerId == aiId);
                            int poolTotal = playerState.ManaPool?.Values.Sum() ?? 0;
                            int remainingToTap = Math.Max(0, manaNeeded - poolTotal);

                            if (remainingToTap > 0)
                            {
                                // Choose untapped lands
                                var battlefieldKey = $"{aiId}_battlefield";
                                var availableLands = session.Zones.ContainsKey(battlefieldKey)
                                    ? session.Zones[battlefieldKey]
                                        .Where(c => c.TypeLine != null &&
                                                    c.TypeLine.IndexOf("Land", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                    !c.IsTapped)
                                        .ToList()
                                    : new List<CardInGame>();

                                // if not enough lands -> skip this action
                                if (availableLands.Count < remainingToTap)
                                {
                                    _logger.LogInformation("[AIPlayTurn] Not enough untapped lands to pay {CardId} (need {Need}, have {Have})",
                                        action.CardId, remainingToTap, availableLands.Count);
                                    continue;
                                }

                                // Tap the required lands one by one
                                var landsToTap = availableLands.Take(remainingToTap).ToList();
                                bool tapFailed = false;

                                foreach (var land in landsToTap)
                                {
                                    try
                                    {
                                        session = await _engine.TapLandAsync(session, aiId, land.CardId);
                                        await _engine.SaveSessionAsync(session);
                                        _logger.LogInformation("[AIPlayTurn] Tapped land {CardName} ({CardId}) for AI", land.Name, land.CardId);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "[AIPlayTurn] Failed to tap land {CardId}.", land.CardId);
                                        tapFailed = true;
                                        break;
                                    }
                                }

                                if (tapFailed)
                                    continue;
                            }

                            // Now try to validate & play
                            try
                            {
                                await _engine.ValidatePlayAsync(session, aiId, action.CardId);
                                session = await _engine.PlayCardAsync(session, aiId, action.CardId);
                                await _engine.SaveSessionAsync(session);

                                _logger.LogInformation("[AIPlayTurn] Played spell {CardId}.", action.CardId);
                                // ✅ Continue loop to try playing more cards
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "[AIPlayTurn] Validate or Play failed for {CardId}.", action.CardId);
                                continue;
                            }

                            break;

                        case ActionType.Discard:
                            if (action.CardsToDiscard != null && action.CardsToDiscard.Count > 0)
                            {
                                session = await _engine.DiscardCards(session, aiId, action.CardsToDiscard, new());
                                await _engine.SaveSessionAsync(session);
                                _logger.LogInformation("[AIPlayTurn] Discarded cards: {Cards}", string.Join(",", action.CardsToDiscard));
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AIPlayTurn] Failed to execute {ActionType} for {CardId}.", action.Type, action.CardId);
                    continue; // ✅ Try next action
                }

                // ✅ REMOVED: if (acted) break;
                // Let the loop continue until AI returns EndTurn or MAX_ITER reached
            }

            // 🟥 End of Turn Phases
            try
            {
                if (_engine.IsCombatPhase(session, aiId))
                    session = _engine.ResolveCombatPhase(session, aiId);

                if (_engine.IsPreEndPhase(session, aiId))
                    session = _engine.PreEndCheck(session, aiId);

                if (!_engine.IsEndPhase(session, aiId))
                    session.CurrentPhase = Phase.End;

                session = _engine.EndTurn(session, aiId);
                await _engine.SaveSessionAsync(session);

                _logger.LogInformation("[AIPlayTurn] Ended turn for AI ({AIId}).", aiId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AIPlayTurn] Error while finishing AI turn.");
                throw;
            }

            // 🧩 Dump final state snapshot
            var battlefieldCount = session.Zones.ContainsKey($"{aiId}_battlefield") ? session.Zones[$"{aiId}_battlefield"].Count : 0;
            var handCount = session.Zones.ContainsKey($"{aiId}_hand") ? session.Zones[$"{aiId}_hand"].Count : 0;
            var mana = session.Players.First(p => p.PlayerId == aiId)?.ManaPool?.ToString() ?? "N/A";

            _logger.LogInformation("[AIPlayTurn][StateDump] => Phase={Phase}, Hand={Hand}, Battlefield={Battlefield}, Mana={Mana}",
                session.CurrentPhase, handCount, battlefieldCount, mana);

            return _mapper.Map<GameSessionDto>(session);
        }
    }
}