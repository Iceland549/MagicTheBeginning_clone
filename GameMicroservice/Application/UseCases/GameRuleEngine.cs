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

        #region Turn flow & draw

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

        #endregion

        #region Land play (validate, apply, landfall)

        public bool IsLandPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main &&
                   s.ActivePlayerId == playerId &&
                   s.Players.First(p => p.PlayerId == playerId).LandsPlayedThisTurn < 1;
        }

        public async Task ValidatePlayLandAsync(GameSession s, string playerId, string cardName)
        {
            if (!IsLandPhase(s, playerId))
                throw new InvalidOperationException("Cannot play land now");

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardName == cardName))
                throw new InvalidOperationException("Card not in hand");

            var card = await _cardClient.GetCardByNameAsync(cardName)
                ?? throw new KeyNotFoundException("Card not found");

            if (!card.TypeLine.Contains("Land"))
                throw new InvalidOperationException("Card is not a land");

            var player = s.Players.First(p => p.PlayerId == playerId);
            if (player.LandsPlayedThisTurn >= 1)
                throw new InvalidOperationException("Only one land per turn allowed");
        }

        // Keep a synchronous helper (existing code uses synchronous PlayLand in several places)
        public GameSession PlayLand(GameSession s, string playerId, string cardName)
        {
            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var card = s.Zones[handKey].FirstOrDefault(c => c.CardName == cardName)
                ?? throw new InvalidOperationException("Card not found in hand");

            // move card: hand -> battlefield
            s.Zones[handKey].Remove(card);
            s.Zones[battlefieldKey].Add(card);

            var player = s.Players.First(p => p.PlayerId == playerId);
            player.LandsPlayedThisTurn++;

            // Ensure the CardInGame has TypeLine (useful if object in session is "light")
            if (string.IsNullOrEmpty(card.TypeLine))
            {
                try
                {
                    var details = _cardClient.GetCardByNameAsync(cardName).GetAwaiter().GetResult();
                    if (details != null)
                    {
                        card.TypeLine = card.TypeLine ?? details.TypeLine;
                        card.ManaCost = card.ManaCost ?? details.ManaCost;
                    }
                }
                catch
                {
                    // best-effort, fail safe: if we can't fetch details, proceed without throwing
                }
            }

            return OnLandfall(s, playerId, cardName);
        }

        // Async wrapper to respect IGameRulesEngine signature if interface expects PlayLandAsync
        public Task<GameSession> PlayLandAsync(GameSession s, string playerId, string cardName)
        {
            var result = PlayLand(s, playerId, cardName);
            return Task.FromResult(result);
        }

        public GameSession OnLandfall(GameSession s, string playerId, string cardName)
        {
            var battlefieldKey = $"{playerId}_battlefield";
            var player = s.Players.First(p => p.PlayerId == playerId);
            var landCard = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardName == cardName);

            if (landCard == null)
                throw new InvalidOperationException("Land not found on battlefield");

            string manaColorKey = "Colorless";
            var typeLine = landCard.TypeLine ?? string.Empty;

            if (typeLine.IndexOf("Forest", StringComparison.OrdinalIgnoreCase) >= 0) manaColorKey = "Green";
            else if (typeLine.IndexOf("Swamp", StringComparison.OrdinalIgnoreCase) >= 0) manaColorKey = "Black";
            else if (typeLine.IndexOf("Plains", StringComparison.OrdinalIgnoreCase) >= 0) manaColorKey = "White";
            else if (typeLine.IndexOf("Mountain", StringComparison.OrdinalIgnoreCase) >= 0) manaColorKey = "Red";
            else if (typeLine.IndexOf("Island", StringComparison.OrdinalIgnoreCase) >= 0) manaColorKey = "Blue";

            player.ManaPool[manaColorKey] = player.ManaPool.GetValueOrDefault(manaColorKey, 0) + 1;
            Console.WriteLine($"[GameRulesEngine] OnLandfall: +1 {manaColorKey} to {playerId} (now {player.ManaPool[manaColorKey]})");
            return s;
        }

        #endregion

        #region Play spells / instants (validate + apply)

        public bool IsMainPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;

        public async Task ValidatePlayAsync(GameSession s, string playerId, string cardName)
        {
            if (!IsMainPhase(s, playerId))
                throw new InvalidOperationException("Not in Main phase");

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardName == cardName))
                throw new InvalidOperationException("Card not in hand");

            var card = await _cardClient.GetCardByNameAsync(cardName)
                ?? throw new KeyNotFoundException("Card not found");

            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
                throw new InvalidOperationException("Insufficient mana to cast this card");
        }

        public async Task<GameSession> PlayCardAsync(GameSession s, string playerId, string cardName)
        {
            await ValidatePlayAsync(s, playerId, cardName);

            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var cardInHand = s.Zones[handKey].FirstOrDefault(c => c.CardName == cardName);
            if (cardInHand == null)
                throw new InvalidOperationException("Card not found in hand");

            // Remove from hand, create in-game instance
            s.Zones[handKey].Remove(cardInHand);
            var cardOnBattlefield = new CardInGame(cardName) { HasSummoningSickness = true };
            s.Zones[battlefieldKey].Add(cardOnBattlefield);

            var cardDetails = await _cardClient.GetCardByNameAsync(cardName)
                ?? throw new KeyNotFoundException("Card not found");

            // Deduct mana
            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, cardDetails.ManaCost);

            // Optionally fill metadata on in-game instance
            cardOnBattlefield.TypeLine = cardDetails.TypeLine;
            cardOnBattlefield.ManaCost = cardDetails.ManaCost;

            return s;
        }

        public bool IsSpellPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Main || s.CurrentPhase == Phase.Combat;

        public async Task ValidateInstantAsync(GameSession s, string playerId, string cardName)
        {
            if (!IsSpellPhase(s, playerId))
                throw new InvalidOperationException("Cannot cast instant now");

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardName == cardName))
                throw new InvalidOperationException("Card not in hand");

            var card = await _cardClient.GetCardByNameAsync(cardName)
                ?? throw new KeyNotFoundException("Card not found");

            if (!card.TypeLine.Contains("Instant"))
                throw new InvalidOperationException("Card is not an instant");

            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
                throw new InvalidOperationException("Insufficient mana to cast this instant");
        }

        public async Task<GameSession> CastInstantAsync(GameSession s, string playerId, string cardName, string? targetId)
        {
            await ValidateInstantAsync(s, playerId, cardName);

            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var cardInHand = s.Zones[handKey].FirstOrDefault(c => c.CardName == cardName)
                ?? throw new InvalidOperationException("Card not in hand");

            s.Zones[handKey].Remove(cardInHand);
            var cardOnBattlefield = new CardInGame(cardName) { HasSummoningSickness = true };
            s.Zones[battlefieldKey].Add(cardOnBattlefield);

            var cardDetails = await _cardClient.GetCardByNameAsync(cardName)
                ?? throw new KeyNotFoundException("Card not found");

            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, cardDetails.ManaCost);

            cardOnBattlefield.TypeLine = cardDetails.TypeLine;
            cardOnBattlefield.ManaCost = cardDetails.ManaCost;

            // targetId handling left to higher-level logic (effects, damage, etc.)
            return s;
        }

        #endregion

        #region Combat / Attack / Block

        public GameSession StartCombatPhase(GameSession s, string playerId)
        {
            if (s.CurrentPhase != Phase.Main || s.ActivePlayerId != playerId)
                throw new InvalidOperationException("Cannot start combat now");
            s.CurrentPhase = Phase.Combat;
            return s;
        }

        public bool IsCombatPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Combat && s.ActivePlayerId == playerId;

        public async Task ValidateAttackAsync(GameSession s, string playerId, List<string> attackers)
        {
            if (!IsCombatPhase(s, playerId))
                throw new InvalidOperationException("Not in Combat phase");

            var battlefieldKey = $"{playerId}_battlefield";
            foreach (var cardName in attackers)
            {
                var cardInGame = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardName == cardName)
                    ?? throw new InvalidOperationException($"Card {cardName} not on battlefield");
                if (cardInGame.IsTapped || cardInGame.HasSummoningSickness)
                    throw new InvalidOperationException($"Card {cardName} cannot attack");

                var details = await _cardClient.GetCardByNameAsync(cardName)
                    ?? throw new KeyNotFoundException($"Card {cardName} not found");
                if (!details.TypeLine.Contains("Creature"))
                    throw new InvalidOperationException($"Card {cardName} is not a creature and cannot attack");
            }
        }

        public async Task<GameSession> ResolveCombatAsync(GameSession s, string playerId, List<string> attackers, Dictionary<string, string> blockers)
        {
            await ValidateAttackAsync(s, playerId, attackers);

            var opponentId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            var opponentBattlefieldKey = $"{opponentId}_battlefield";
            var myBattlefieldKey = $"{playerId}_battlefield";
            var graveyardKey = $"{playerId}_graveyard";
            var opponentGraveyardKey = $"{opponentId}_graveyard";

            // Handle pairwise attacker vs blocker exchanges
            foreach (var kvp in blockers)
            {
                var attackerName = kvp.Key;
                var blockerName = kvp.Value;

                var attackerCard = s.Zones[myBattlefieldKey].FirstOrDefault(c => c.CardName == attackerName)
                    ?? throw new InvalidOperationException($"Attacker {attackerName} not on battlefield");
                var blockerCard = s.Zones[opponentBattlefieldKey].FirstOrDefault(c => c.CardName == blockerName)
                    ?? throw new InvalidOperationException($"Blocker {blockerName} not on battlefield");

                var attackerDetails = await _cardClient.GetCardByNameAsync(attackerName)
                    ?? throw new KeyNotFoundException("Attacker card not found");
                var blockerDetails = await _cardClient.GetCardByNameAsync(blockerName)
                    ?? throw new KeyNotFoundException("Blocker card not found");

                if ((attackerDetails.Power ?? 0) >= (blockerDetails.Toughness ?? int.MaxValue))
                {
                    s.Zones[opponentBattlefieldKey].Remove(blockerCard);
                    s.Zones[opponentGraveyardKey].Add(blockerCard);
                }

                if ((blockerDetails.Power ?? 0) >= (attackerDetails.Toughness ?? int.MaxValue))
                {
                    s.Zones[myBattlefieldKey].Remove(attackerCard);
                    s.Zones[graveyardKey].Add(attackerCard);
                }
            }

            // Unblocked attackers deal damage to opponent player
            var unblocked = attackers.Where(a => !blockers.ContainsKey(a)).ToList();
            var opponent = s.Players.First(p => p.PlayerId == opponentId);
            foreach (var attackerName in unblocked)
            {
                var attackerDetails = await _cardClient.GetCardByNameAsync(attackerName)
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

        #endregion

        #region Pre-end / end-turn / blocks

        public bool IsPreEndPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;

        public GameSession PreEndCheck(GameSession s, string playerId)
        {
            if (!IsPreEndPhase(s, playerId))
                throw new InvalidOperationException("Not in Pre-End phase");
            return s;
        }

        public bool IsEndPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.End && s.ActivePlayerId == playerId;

        public GameSession EndTurn(GameSession s, string playerId)
        {
            if (!IsEndPhase(s, playerId))
                throw new InvalidOperationException("Not in End phase");

            s.ActivePlayerId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            s.CurrentPhase = Phase.Draw;

            foreach (var p in s.Players)
            {
                p.HasDrawnThisTurn = false;
                p.LandsPlayedThisTurn = 0;
            }

            return s;
        }

        public bool IsBlockPhase(GameSession session, string playerId)
            => session.CurrentPhase == Phase.Combat && session.ActivePlayerId != playerId;

        public async Task ValidateBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            if (!IsBlockPhase(session, playerId))
                throw new InvalidOperationException("Not in Block phase");

            var battlefieldKey = $"{playerId}_battlefield";

            foreach (var blocker in blockers.Values)
            {
                var blockerCard = session.Zones[battlefieldKey].FirstOrDefault(c => c.CardName == blocker);
                if (blockerCard == null)
                    throw new InvalidOperationException($"Blocker {blocker} not on battlefield");
                if (blockerCard.IsTapped)
                    throw new InvalidOperationException($"Blocker {blocker} is tapped");

                var details = await _cardClient.GetCardByNameAsync(blocker)
                    ?? throw new KeyNotFoundException("Blocker not found");
                if (details.TypeLine == null || !details.TypeLine.Contains("Creature"))
                    throw new InvalidOperationException($"Blocker {blocker} is not a creature");
            }
        }

        public Task<GameSession> ResolveBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            // For now delegate to ResolveCombatAsync in game flow; keep API compliant
            return Task.FromResult(session);
        }

        public async Task<GameSession> DiscardCards(GameSession session, string playerId, List<string> cardsToDiscard, Dictionary<string, string> blockers)
        {
            var opponentId = session.PlayerOneId == playerId ? session.PlayerTwoId : session.PlayerOneId;

            foreach (var kvp in blockers)
            {
                var attackerId = kvp.Key;
                var blockerId = kvp.Value;

                var attacker = session.Zones[$"{opponentId}_battlefield"].FirstOrDefault(c => c.CardName == attackerId);
                var blocker = session.Zones[$"{playerId}_battlefield"].FirstOrDefault(c => c.CardName == blockerId);

                if (attacker == null || blocker == null) continue;

                var attackerDetails = await _cardClient.GetCardByNameAsync(attackerId) ?? throw new InvalidOperationException("Attacker not found");
                var blockerDetails = await _cardClient.GetCardByNameAsync(blockerId) ?? throw new InvalidOperationException("Blocker not found");

                if ((attackerDetails.Power ?? 0) >= (blockerDetails.Toughness ?? int.MaxValue))
                {
                    session.Zones[$"{playerId}_battlefield"].Remove(blocker);
                    session.Zones[$"{playerId}_graveyard"].Add(blocker);
                }
                if ((blockerDetails.Power ?? 0) >= (attackerDetails.Toughness ?? int.MaxValue))
                {
                    session.Zones[$"{opponentId}_battlefield"].Remove(attacker);
                    session.Zones[$"{opponentId}_graveyard"].Add(attacker);
                }
            }

            return session;
        }

        #endregion

        #region Load / Save / Endgame

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

        public EndGameDto? CheckEndGame(GameSession session)
        {
            var loser = session.Players.FirstOrDefault(p => p.LifeTotal <= 0);
            if (loser != null)
            {
                var winner = session.Players.First(p => p.PlayerId != loser.PlayerId);
                return new EndGameDto
                {
                    WinnerId = winner.PlayerId,
                    Reason = $"Le joueur {loser.PlayerId} a 0 PV."
                };
            }

            foreach (var player in session.Players)
            {
                var libraryKey = $"{player.PlayerId}_library";
                if (session.Zones[libraryKey].Count == 0 && !player.HasDrawnThisTurn)
                {
                    var winner = session.Players.First(p => p.PlayerId != player.PlayerId);
                    return new EndGameDto
                    {
                        WinnerId = winner.PlayerId,
                        Reason = $"Le joueur {player.PlayerId} n’a plus de cartes à piocher."
                    };
                }
            }

            return null;
        }

        #endregion

        #region Mana parsing & helpers

        // Parse mana cost into a dictionary of required symbols:
        // keys: "White","Blue","Black","Red","Green","Colorless","GENERIC"
        // Accept formats like "{G}{G}", "{3}{G}", "2GG", etc.
        private Dictionary<string, int> ParseManaCost(string manaCost)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(manaCost))
                return dict;

            // If braces format present -> parse tokens inside braces
            if (manaCost.Contains("{"))
            {
                int i = 0;
                while (i < manaCost.Length)
                {
                    if (manaCost[i] == '{')
                    {
                        int j = manaCost.IndexOf('}', i + 1);
                        if (j < 0) break;
                        var token = manaCost.Substring(i + 1, j - i - 1);
                        if (int.TryParse(token, out int num)) dict["GENERIC"] = dict.GetValueOrDefault("GENERIC", 0) + num;
                        else
                        {
                            string color = token switch
                            {
                                "W" => "White",
                                "U" => "Blue",
                                "B" => "Black",
                                "R" => "Red",
                                "G" => "Green",
                                "C" => "Colorless",
                                _ => throw new InvalidOperationException($"Unknown mana symbol: {token}")
                            };
                            dict[color] = dict.GetValueOrDefault(color, 0) + 1;
                        }
                        i = j + 1;
                    }
                    else i++;
                }
            }
            else
            {
                // fallback for compact strings like "2GG" or "3G"
                int i = 0;
                while (i < manaCost.Length)
                {
                    if (char.IsDigit(manaCost[i]))
                    {
                        int j = i;
                        while (j < manaCost.Length && char.IsDigit(manaCost[j])) j++;
                        var num = int.Parse(manaCost.Substring(i, j - i));
                        dict["GENERIC"] = dict.GetValueOrDefault("GENERIC", 0) + num;
                        i = j;
                    }
                    else
                    {
                        var symbol = manaCost[i].ToString().ToUpper();
                        string color = symbol switch
                        {
                            "W" => "White",
                            "U" => "Blue",
                            "B" => "Black",
                            "R" => "Red",
                            "G" => "Green",
                            "C" => "Colorless",
                            _ => throw new InvalidOperationException($"Unknown mana symbol: {symbol}")
                        };
                        dict[color] = dict.GetValueOrDefault(color, 0) + 1;
                        i++;
                    }
                }
            }

            return dict;
        }

        private bool CanPayManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return true;

            var required = ParseManaCost(manaCost);

            // check colored requirements first
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                if (manaPool.GetValueOrDefault(kvp.Key) < kvp.Value)
                    return false;
            }

            // check generic: total mana available minus colored used must be >= generic
            var generic = required.GetValueOrDefault("GENERIC", 0);
            var totalAvailable = manaPool.Values.Sum();
            var coloredUsed = required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)).Sum(k => k.Value);
            return totalAvailable - coloredUsed >= generic;
        }

        private void DeductManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return;

            var required = ParseManaCost(manaCost);

            // Deduct colored mana first
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                var color = kvp.Key;
                var need = kvp.Value;
                if (!manaPool.ContainsKey(color) || manaPool[color] < need)
                    throw new InvalidOperationException($"Insufficient {color} mana");
                manaPool[color] -= need;
            }

            // Deduct generic from any remaining mana (deterministic order)
            var generic = required.GetValueOrDefault("GENERIC", 0);
            var consumeOrder = new[] { "White", "Blue", "Black", "Red", "Green", "Colorless" }
                .Where(k => manaPool.ContainsKey(k)).ToList();

            foreach (var color in consumeOrder)
            {
                if (generic <= 0) break;
                var use = Math.Min(manaPool[color], generic);
                manaPool[color] -= use;
                generic -= use;
            }

            if (generic > 0)
                throw new InvalidOperationException("Insufficient generic mana");
        }

        private int TotalManaCost(string manaCost)
        {
            var parsed = ParseManaCost(manaCost);
            return parsed.Values.Sum();
        }

        #endregion
    }
}
