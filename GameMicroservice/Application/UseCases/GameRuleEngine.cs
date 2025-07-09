using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class GameRulesEngine : IGameRulesEngine
    {
        private readonly ICardClient _cardClient;
        private readonly IGameSessionRepository _gameSessionRepository;

        public GameRulesEngine(ICardClient cardClient, IGameSessionRepository gameSessionRepository)
        {
            _cardClient = cardClient;
            _gameSessionRepository = gameSessionRepository;
        }

        public bool HasDrawnThisTurn(GameSession s, string playerId)
        {
            var player = s.Players.FirstOrDefault(p => p.PlayerId == playerId)
                ?? throw new InvalidOperationException("Player not found");
            return player.HasDrawnThisTurn;
        }

        public GameSession DrawStep(GameSession s, string playerId)
        {
            if (s.CurrentPhase != Phase.Draw)
                throw new InvalidOperationException("Not in Draw phase");
            if (HasDrawnThisTurn(s, playerId))
                throw new InvalidOperationException("Player already drew this turn");

            var libraryKey = $"{playerId}_library";
            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(libraryKey) || s.Zones[libraryKey].Count == 0)
                throw new InvalidOperationException("Library is empty");

            var card = s.Zones[libraryKey][0];
            s.Zones[libraryKey].RemoveAt(0);
            s.Zones[handKey].Add(card);

            var player = s.Players.First(p => p.PlayerId == playerId);
            player.HasDrawnThisTurn = true;
            s.CurrentPhase = Phase.Main;

            return s;
        }

        public bool IsLandPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId && s.Players.First(p => p.PlayerId == playerId).LandsPlayedThisTurn < 1;
        }

        public async Task ValidatePlayLandAsync(GameSession s, string playerId, string cardId)
        {
            if (!IsLandPhase(s, playerId))
                throw new InvalidOperationException("Cannot play land now");
            var handKey = $"{playerId}_hand";
            if (!s.Zones[handKey].Any(c => c.CardId == cardId))
                throw new InvalidOperationException("Card not in hand");
            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found");
            if (!card.TypeLine.Contains("Land"))
                throw new InvalidOperationException("Card is not a land");

            var player = s.Players.First(p => p.PlayerId == playerId);
            if (player.LandsPlayedThisTurn >= 1)
                throw new InvalidOperationException("Only one land per turn allowed");
        }

        public GameSession PlayLand(GameSession s, string playerId, string cardId)
        {
            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var card = s.Zones[handKey].FirstOrDefault(c => c.CardId == cardId)
                ?? throw new InvalidOperationException("Card not found in hand");

            s.Zones[handKey].Remove(card);
            s.Zones[battlefieldKey].Add(card);

            var player = s.Players.First(p => p.PlayerId == playerId);
            player.LandsPlayedThisTurn++;

            return OnLandfall(s, playerId, cardId);
        }

        public GameSession OnLandfall(GameSession s, string playerId, string cardId)
        {
            var player = s.Players.First(p => p.PlayerId == playerId);
            player.ManaPool["Colorless"] = player.ManaPool.GetValueOrDefault("Colorless", 0) + 1;
            return s;
        }

        public bool IsMainPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;
        }

        public async Task ValidatePlayAsync(GameSession s, string playerId, string cardId)
        {
            if (!IsMainPhase(s, playerId))
                throw new InvalidOperationException("Not in Main phase");
            var handKey = $"{playerId}_hand";
            if (!s.Zones[handKey].Any(c => c.CardId == cardId))
                throw new InvalidOperationException("Card not in hand");

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found");
            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
                throw new InvalidOperationException("Insufficient mana to cast this card");
        }

        public async Task<GameSession> PlayCardAsync(GameSession s, string playerId, string cardId)
        {
            await ValidatePlayAsync(s, playerId, cardId);
            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";
            var cardInHand = s.Zones[handKey].First(c => c.CardId == cardId);
            if (cardInHand == null)
                throw new InvalidOperationException("Card not found in hand");
            s.Zones[handKey].Remove(cardInHand);
            s.Zones[battlefieldKey].Add(new CardInGame(cardId));

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found");
            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost);
            return s;
        }

        public bool IsSpellPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main || s.CurrentPhase == Phase.Combat;
        }

        public async Task ValidateInstantAsync(GameSession s, string playerId, string cardId)
        {
            if (!IsSpellPhase(s, playerId))
                throw new InvalidOperationException("Cannot cast instant now");
            var handKey = $"{playerId}_hand";
            if (!s.Zones[handKey].Any(c => c.CardId == cardId))
                throw new InvalidOperationException("Card not in hand");

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found");
            if (!card.TypeLine.Contains("Instant"))
                throw new InvalidOperationException("Card is not an instant");

            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
                throw new InvalidOperationException("Insufficient mana to cast this instant");
        }

        public async Task<GameSession> CastInstantAsync(GameSession s, string playerId, string cardId, string? targetId)
        {
            await ValidateInstantAsync(s, playerId, cardId);
            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";
            var cardInHand = s.Zones[handKey].First(c => c.CardId == cardId);
            s.Zones[handKey].Remove(cardInHand);
            s.Zones[battlefieldKey].Add(new CardInGame(cardId));

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found");
            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost);
            return s;
        }

        public GameSession StartCombatPhase(GameSession s, string playerId)
        {
            if (s.CurrentPhase != Phase.Main || s.ActivePlayerId != playerId)
                throw new InvalidOperationException("Cannot start combat now");
            s.CurrentPhase = Phase.Combat;
            return s;
        }

        public bool IsCombatPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Combat && s.ActivePlayerId == playerId;
        }

        public async Task ValidateAttackAsync(GameSession s, string playerId, List<string> attackers)
        {
            if (!IsCombatPhase(s, playerId))
                throw new InvalidOperationException("Not in Combat phase");
            var battlefieldKey = $"{playerId}_battlefield";
            foreach (var cardId in attackers)
            {
                var cardInGame = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardId == cardId)
                    ?? throw new InvalidOperationException($"Card {cardId} not on battlefield");
                if (cardInGame.IsTapped || cardInGame.HasSummoningSickness)
                    throw new InvalidOperationException($"Card {cardId} cannot attack");
                // Validation asynchrone : vérifier que la carte est une créature
                var cardDetails = await _cardClient.GetCardByIdAsync(cardId)
                    ?? throw new KeyNotFoundException($"Card {cardId} not found");
                if (!cardDetails.TypeLine.Contains("Creature"))
                    throw new InvalidOperationException($"Card {cardId} is not a creature and cannot attack");
            }
        }

        public async Task<GameSession> ResolveCombatAsync(GameSession s, string playerId, List<string> attackers, Dictionary<string, string> blockers)
        {
            await ValidateAttackAsync(s, playerId, attackers);
            var opponentId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            var opponentBattlefieldKey = $"{opponentId}_battlefield";
            var graveyardKey = $"{playerId}_graveyard";
            var opponentGraveyardKey = $"{opponentId}_graveyard";

            foreach (var blocker in blockers)
            {
                var blockerCard = s.Zones[opponentBattlefieldKey].FirstOrDefault(c => c.CardId == blocker.Value)
                    ?? throw new InvalidOperationException($"Blocker {blocker.Value} not on battlefield");
                var attackerCard = s.Zones[$"{playerId}_battlefield"].FirstOrDefault(c => c.CardId == blocker.Key)
                    ?? throw new InvalidOperationException($"Attacker {blocker.Key} not on battlefield");

                var attackerDetails = await _cardClient.GetCardByIdAsync(blocker.Key)
                    ?? throw new KeyNotFoundException("Attacker card not found");
                var blockerDetails = await _cardClient.GetCardByIdAsync(blocker.Value)
                    ?? throw new KeyNotFoundException("Blocker card not found");

                if (attackerDetails.Power >= blockerDetails.Toughness)
                {
                    s.Zones[opponentBattlefieldKey].Remove(blockerCard);
                    s.Zones[opponentGraveyardKey].Add(blockerCard);
                }
                if (blockerDetails.Power >= attackerDetails.Toughness)
                {
                    s.Zones[$"{playerId}_battlefield"].Remove(attackerCard);
                    s.Zones[graveyardKey].Add(attackerCard);
                }
            }

            var unblockedAttackers = attackers.Where(a => !blockers.ContainsKey(a)).ToList();
            var opponent = s.Players.First(p => p.PlayerId == opponentId);
            foreach (var attackerId in unblockedAttackers)
            {
                var attackerDetails = await _cardClient.GetCardByIdAsync(attackerId)
                    ?? throw new KeyNotFoundException("Attacker card not found");
                opponent.LifeTotal -= attackerDetails.Power ?? 0;
            }

            return s;
        }

        public GameSession ResolveCombatPhase(GameSession s, string playerId)
        {
            if (!IsCombatPhase(s, playerId))
                throw new InvalidOperationException("Not in Combat phase");
            s.CurrentPhase = Phase.Main;
            return s;
        }

        public bool IsPreEndPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;
        }

        public GameSession PreEndCheck(GameSession s, string playerId)
        {
            if (!IsPreEndPhase(s, playerId))
                throw new InvalidOperationException("Not in Pre-End phase");
            return s;
        }

        public bool IsEndPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.End && s.ActivePlayerId == playerId;
        }

        public GameSession EndTurn(GameSession s, string playerId)
        {
            if (!IsEndPhase(s, playerId))
                throw new InvalidOperationException("Not in End phase");
            s.ActivePlayerId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            s.CurrentPhase = Phase.Draw;

            foreach (var player in s.Players)
            {
                player.HasDrawnThisTurn = false;
                player.LandsPlayedThisTurn = 0;
            }
            return s;
        }

        public async Task<GameSession> LoadSessionAsync(string sessionId)
        {
            var session = await _gameSessionRepository.GetByIdAsync(sessionId)
                ?? throw new InvalidOperationException($"Session {sessionId} not found");
            return session;
        }

        public async Task SaveSessionAsync(GameSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            await _gameSessionRepository.UpdateAsync(session);
        }

        private bool CanPayManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return true;

            var requiredMana = new Dictionary<string, int>();
            var genericMana = 0;

            foreach (var c in manaCost.Replace("{", "").Split('}'))
            {
                if (string.IsNullOrEmpty(c)) continue;
                if (int.TryParse(c, out int value))
                    genericMana += value;
                else
                {
                    string color;
                    switch (c)
                    {
                        case "W": color = "White"; break;
                        case "U": color = "Blue"; break;
                        case "B": color = "Black"; break;
                        case "R": color = "Red"; break;
                        case "G": color = "Green"; break;
                        case "C": color = "Colorless"; break;
                        default: throw new InvalidOperationException($"Unknown mana color: {c}");
                    }
                    requiredMana[color] = requiredMana.GetValueOrDefault(color) + 1;
                }
            }

            foreach (var (color, count) in requiredMana)
            {
                if (manaPool.GetValueOrDefault(color) < count)
                    return false;
            }

            var totalMana = manaPool.Values.Sum();
            var specificManaUsed = requiredMana.Values.Sum();
            return totalMana - specificManaUsed >= genericMana;
        }


        private void DeductManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return;

            var requiredMana = new Dictionary<string, int>();
            var genericMana = 0;

            foreach (var c in manaCost.Replace("{", "").Split('}'))
            {
                if (string.IsNullOrEmpty(c)) continue;
                if (int.TryParse(c, out int value))
                    genericMana += value;
                else
                {
                    string color;
                    switch (c)
                    {
                        case "W": color = "White"; break;
                        case "U": color = "Blue"; break;
                        case "B": color = "Black"; break;
                        case "R": color = "Red"; break;
                        case "G": color = "Green"; break;
                        case "C": color = "Colorless"; break;
                        default: throw new InvalidOperationException($"Unknown mana color: {c}");
                    }
                    requiredMana[color] = requiredMana.GetValueOrDefault(color) + 1;
                }
            }

            foreach (var (color, count) in requiredMana)
            {
                manaPool[color] -= count;
                if (manaPool[color] < 0)
                    throw new InvalidOperationException("Insufficient mana");
            }

            var totalMana = manaPool.Values.Sum();
            if (totalMana < genericMana)
                throw new InvalidOperationException("Insufficient generic mana");

            foreach (var color in manaPool.Keys.ToList())
            {
                if (genericMana <= 0) break;
                var deduct = Math.Min(manaPool[color], genericMana);
                manaPool[color] -= deduct;
                genericMana -= deduct;
            }
        }
        public bool IsBlockPhase(GameSession session, string playerId)
        {
            // Implémentation temporaire
            return false;
        }

        public async Task ValidateBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            // Implémentation temporaire
            await Task.CompletedTask;
        }

        public Task<GameSession> ResolveBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            return Task.FromResult(session);
        }


        public GameSession DiscardCards(GameSession session, string playerId, List<string> cardsToDiscard)
        {
            // Implémentation temporaire
            return session;
        }

        public EndGameDto? CheckEndGame(GameSession session)
        {
            // Implémentation temporaire
            return null;
        }


    }
}