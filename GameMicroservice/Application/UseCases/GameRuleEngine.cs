using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Helpers;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    /// <summary>
    /// Game rules engine with detailed logging.
    /// </summary>
    public class GameRulesEngine : IGameRulesEngine
    {
        private readonly ILogger<GameRulesEngine> _logger;
        private readonly ICardClient _cardClient;
        private readonly IGameSessionRepository _gameSessionRepository;

        public GameRulesEngine(ICardClient cardClient, IGameSessionRepository gameSessionRepository, ILogger<GameRulesEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cardClient = cardClient ?? throw new ArgumentException(nameof(cardClient));
            _gameSessionRepository = gameSessionRepository ?? throw new ArgumentException(nameof(gameSessionRepository));
        }

        // ==========================================      ==========================================

        #region Turn flow & draw

        public bool HasDrawnThisTurn(GameSession s, string playerId)
        {
            var player = s.Players.FirstOrDefault(p => p.PlayerId == playerId)
                ?? throw new InvalidOperationException("Player not found");
            return player.HasDrawnThisTurn;
        }

        /// <summary>
        /// Tap a land to add mana to the pool.
        /// </summary>
        public async Task<GameSession> TapLandAsync(GameSession s, string playerId, string cardId)
        {
            _logger.LogDebug("[TapLand] BEGIN for PlayerId={PlayerId}, CardId={CardId}", playerId, cardId);

            var battlefieldKey = $"{playerId}_battlefield";
            if (!s.Zones.ContainsKey(battlefieldKey))
            {
                _logger.LogWarning("[TapLand] Battlefield zone missing for player {PlayerId}", playerId);
                throw new InvalidOperationException("Battlefield not initialized");
            }

            var land = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardId == cardId && !c.IsTapped);
            if (land == null)
            {
                _logger.LogWarning("[TapLand] Land not found or already tapped for {CardId}", cardId);
                throw new InvalidOperationException("Land not found or already tapped.");
            }

            var details = await _cardClient.GetCardByIdAsync(cardId);
            if (details == null || !(details.TypeLine?.Contains("Land", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                _logger.LogWarning("[TapLand] Not a land card: {CardId}", cardId);
                throw new InvalidOperationException("Not a land.");
            }

            // Resolve available colors: prefer explicit AvailableManaColors stored on the card; if empty derive inline (no extra method)
            var available = (land.AvailableManaColors != null && land.AvailableManaColors.Any())
                ? land.AvailableManaColors
                : (details.TypeLine ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t =>
                        t.Contains("Forest", StringComparison.OrdinalIgnoreCase) ? "Green" :
                        t.Contains("Swamp", StringComparison.OrdinalIgnoreCase) ? "Black" :
                        t.Contains("Plains", StringComparison.OrdinalIgnoreCase) ? "White" :
                        t.Contains("Mountain", StringComparison.OrdinalIgnoreCase) ? "Red" :
                        t.Contains("Island", StringComparison.OrdinalIgnoreCase) ? "Blue" :
                        null
                    )
                    .Where(c => c != null)
                    .Select(c => c!) 
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            if (available == null || available.Count == 0)
                available = new List<string> { "Colorless" };

            // Determine effective color: prefer explicit ChosenLandColor if valid, else use heuristic constrained to available
            string? manaColorKey = null;
            if (!string.IsNullOrEmpty(land.ChosenLandColor) && available.Contains(land.ChosenLandColor, StringComparer.OrdinalIgnoreCase))
            {
                manaColorKey = land.ChosenLandColor;
                _logger.LogDebug("[TapLand] Using existing ChosenLandColor {Color} for {CardId}", manaColorKey, cardId);
            }
            else
            {
                manaColorKey = ChooseBestLandColor(s, playerId, available);
                land.ChosenLandColor = manaColorKey; // persist chosen color for UI/diagnostics
                _logger.LogDebug("[TapLand] Heuristic selected {Color} for {CardId}", manaColorKey, cardId);
            }

            // Tap and add mana exactly once
            land.IsTapped = true;
            _logger.LogDebug("[TapLand] Tapped {CardId}", cardId);

            var player = s.Players.First(p => p.PlayerId == playerId);
            if (player.ManaPool == null) player.ManaPool = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var before = new Dictionary<string, int>(player.ManaPool);
            player.ManaPool[manaColorKey] = player.ManaPool.GetValueOrDefault(manaColorKey, 0) + 1;
            _logger.LogInformation("[TapLand] Added 1 {Color} mana for player {PlayerId}. Pool before={Before} after={After}", manaColorKey, playerId, before, player.ManaPool);
            _logger.LogInformation("[TapLand][StateDump] Player {PlayerId} ManaPool={@Pool}, LandsPlayedThisTurn={Lands}",
                playerId, player.ManaPool, player.LandsPlayedThisTurn);

            return s;
        }


        // ==========================================      ==========================================

        public async Task<GameSession> DrawStepAsync(GameSession s, string playerId)
        {
            var log = Log.ForContext("Method", nameof(DrawStepAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("Phase", s.CurrentPhase);

            log.Information("🟦 BEGIN DrawStepAsync");

            if (s.CurrentPhase != Phase.Draw)
            {
                log.Warning("Invalid phase for draw step: {Phase}", s.CurrentPhase);
                throw new InvalidOperationException("Not in Draw phase");
            }

            if (HasDrawnThisTurn(s, playerId))
            {
                log.Warning("Player {PlayerId} already drew this turn", playerId);
                throw new InvalidOperationException("Player already drew this turn");
            }

            // Untap step before draw
            var battlefieldKey = $"{playerId}_battlefield";
            if (s.Zones.ContainsKey(battlefieldKey))
            {
                foreach (var bfCard in s.Zones[battlefieldKey])
                {
                    bfCard.IsTapped = false;
                    bfCard.HasSummoningSickness = bfCard.TypeLine?.Contains("Creature") ?? false ? false : bfCard.HasSummoningSickness; // Reset seulement pour créatures
                    _logger.LogDebug("[UntapStep] Untapped {CardId} ({Name})", bfCard.CardId, bfCard.Name);
                }
            }

            // Draw
            var libraryKey = $"{playerId}_library";
            var handKey = $"{playerId}_hand";

            if (!s.Zones.ContainsKey(libraryKey) || s.Zones[libraryKey].Count == 0)
            {
                log.Warning("Library empty for player {PlayerId}", playerId);
                throw new InvalidOperationException("Library is empty");
            }

            var card = s.Zones[libraryKey][0];
            s.Zones[libraryKey].RemoveAt(0);

            // Enrich metadata
            if (string.IsNullOrEmpty(card.Name) || string.IsNullOrEmpty(card.TypeLine))
            {
                var details = await _cardClient.GetCardByIdAsync(card.CardId);
                if (details != null)
                {
                    card.Name = details.Name ?? "Unknown";
                    card.TypeLine = details.TypeLine;
                    card.ManaCost = details.ManaCost;
                    card.ImageUrl = details.ImageUrl;
                    card.Power = details.Power;
                    card.Toughness = details.Toughness;
                }
                else
                    log.Warning("Metadata not found for drawn card {CardId}", card.CardId);
                    log.Warning("No details for card {CardId}", card.CardId);
            }

            s.Zones[handKey].Add(card);
            var player = s.Players.First(p => p.PlayerId == playerId);
            player.HasDrawnThisTurn = true;
            s.CurrentPhase = Phase.Main;

            log.Information("✅ Player {PlayerId} drew {CardName} [{CardId}] — Hand now has {Count} cards",
                playerId, card.Name ?? "Unknown", card.CardId, s.Zones[handKey].Count);
            log.Information("🟩 END DrawStepAsync — phase set to MAIN");
            return s;
        }

        #endregion

        // ==========================================      ==========================================

        #region Land play (validate, apply, landfall)

        public bool IsLandPhase(GameSession s, string playerId)
        {
            return s.CurrentPhase == Phase.Main &&
                   s.ActivePlayerId == playerId &&
                   s.Players.First(p => p.PlayerId == playerId).LandsPlayedThisTurn < 1;
        }

        public async Task ValidatePlayLandAsync(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(ValidatePlayLandAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);

            log.Information("🟨 BEGIN ValidatePlayLandAsync");

            if (!IsLandPhase(s, playerId))
            {
                log.Warning("Not in land phase or already played a land.");
                throw new InvalidOperationException("Cannot play land now");
            }

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardId == cardId))
            {
                log.Warning("Card {CardId} not in hand for player {PlayerId}", cardId, playerId);
                throw new InvalidOperationException("Card not in hand");
            }

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found in database");

            if (card.TypeLine == null || !card.TypeLine.Contains("Land", StringComparison.OrdinalIgnoreCase))
            {
                log.Warning("Card {CardId} is not a land (TypeLine={TypeLine})", cardId, card.TypeLine);
                throw new InvalidOperationException("Card is not a land");
            }

            var player = s.Players.First(p => p.PlayerId == playerId);
            if (player.LandsPlayedThisTurn >= 1)
            {
                log.Warning("Player {PlayerId} already played a land this turn", playerId);
                throw new InvalidOperationException("Only one land per turn allowed");
            }

            log.Information("✅ Validation OK: Player {PlayerId} can play land {CardName} [{CardId}]", playerId, card.Name, cardId);
            log.Information("🟩 END ValidatePlayLandAsync");
        }

        public GameSession PlayLand(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(PlayLand))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);
            log.Information("🟨 BEGIN PlayLand");

            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var card = s.Zones[handKey].FirstOrDefault(c => c.CardId == cardId)
                ?? throw new InvalidOperationException("Card not found in hand");

            log.Debug("Moving card {CardName} from hand to battlefield", card.Name);

            s.Zones[handKey].Remove(card);
            s.Zones[battlefieldKey].Add(card);

            card.IsTapped = s.ActivePlayerId != playerId; ;
                log.Information("[PlayLand] Land {CardId} is now tapped upon entering battlefield.", cardId);

            var player = s.Players.First(p => p.PlayerId == playerId);
            player.LandsPlayedThisTurn++;

            log.Information("Land played successfully. LandsPlayedThisTurn={Count}", player.LandsPlayedThisTurn);

            // Fill metadata if missing
            if (string.IsNullOrEmpty(card.TypeLine))
            {
                try
                {
                    var details = _cardClient.GetCardByIdAsync(cardId).GetAwaiter().GetResult();
                    if (details != null)
                    {
                        card.TypeLine = details.TypeLine;
                        card.ManaCost = details.ManaCost;
                        log.Debug("Metadata updated for {CardId}: Type={TypeLine}", cardId, details.TypeLine);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error fetching metadata for card {CardId}", cardId);
                }
            }

            var result = OnLandfall(s, playerId, cardId);
            log.Information("🟩 END PlayLand");
            return result;
        }

        public GameSession OnLandfall(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(OnLandfall))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);
            log.Information("🌿 BEGIN OnLandfall");

            var battlefieldKey = $"{playerId}_battlefield";
            var landCard = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardId == cardId);
            if (landCard == null)
            {
                log.Error("Land {CardId} not found on battlefield", cardId);
                throw new InvalidOperationException("Land not found on battlefield");
            }
            landCard.IsTapped = false;
            landCard.CanBeTapped = true;

            log.Information("[OnLandfall] Land {CardName} set tappable: IsTapped={IsTapped}, CanBeTapped={CanBeTapped}",
                landCard.Name, landCard.IsTapped, landCard.CanBeTapped);

            var typeLine = landCard.TypeLine ?? string.Empty;
            var possibleColors = new List<string>();

            if (typeLine.Contains("Forest", StringComparison.OrdinalIgnoreCase)) possibleColors.Add("Green");
            if (typeLine.Contains("Swamp", StringComparison.OrdinalIgnoreCase)) possibleColors.Add("Black");
            if (typeLine.Contains("Plains", StringComparison.OrdinalIgnoreCase)) possibleColors.Add("White");
            if (typeLine.Contains("Mountain", StringComparison.OrdinalIgnoreCase)) possibleColors.Add("Red");
            if (typeLine.Contains("Island", StringComparison.OrdinalIgnoreCase)) possibleColors.Add("Blue");
            if (possibleColors.Count == 0) possibleColors.Add("Colorless");

            landCard.AvailableManaColors = possibleColors;

            var player = s.Players.First(p => p.PlayerId == playerId);
            foreach (var color in possibleColors)
            {
                landCard.AvailableManaColors = possibleColors;
                log.Information("✅ Landfall: {CardName} can produce {Colors}", landCard.Name ?? cardId, string.Join(", ", possibleColors));
            }

            log.Information("✅ Landfall: {CardName} now produces {Colors}", landCard.Name ?? cardId, string.Join(", ", possibleColors));
            log.Information("🌿 END OnLandfall");
            return s;
        }

        #endregion

        // ==========================================      ==========================================

        #region Play spells / instants (validate + apply)

        public bool IsMainPhase(GameSession s, string playerId)
        {
            bool result = s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;
            Log.Debug("[IsMainPhase] Player={PlayerId}, Result={Result}, Phase={Phase}", playerId, result, s.CurrentPhase);
            return result;
        }

        public async Task ValidatePlayAsync(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(ValidatePlayAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);

            log.Information("🎴 BEGIN ValidatePlayAsync");

            if (!IsMainPhase(s, playerId))
            {
                log.Warning("❌ Not in main phase or inactive player");
                throw new InvalidOperationException("Not in Main phase");
            }

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardId == cardId))
            {
                log.Warning("Card {CardId} not in hand", cardId);
                throw new InvalidOperationException("Card not in hand");
            }

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Card not found in Card service");

            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
            {
                log.Warning("❌ Insufficient mana to cast {CardName} ({ManaCost})", card.Name, card.ManaCost);
                throw new InvalidOperationException("Insufficient mana to cast this card");
            }

            log.Information("✅ Player {PlayerId} can legally play {CardName} ({CardId})", playerId, card.Name, cardId);
            log.Information("🎴 END ValidatePlayAsync");
        }

        public async Task<GameSession> PlayCardAsync(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(PlayCardAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);
            log.Information("✨ BEGIN PlayCardAsync");

            await ValidatePlayAsync(s, playerId, cardId);

            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var cardInHand = s.Zones[handKey].FirstOrDefault(c => c.CardId == cardId);
            if (cardInHand == null)
            {
                log.Error("Card {CardId} not found in hand after validation", cardId);
                throw new InvalidOperationException("Card not found in hand");
            }

            var cardDetails = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException($"Card metadata for {cardId} not found");

            // Deduct mana cost
            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, cardDetails.ManaCost);
            log.Information("Mana cost {ManaCost} paid successfully for {CardName}", cardDetails.ManaCost, cardDetails.Name);

            // Remove from hand
            s.Zones[handKey].Remove(cardInHand);
            log.Debug("Card {CardName} removed from hand", cardDetails.Name);

            var cardOnBattlefield = new CardInGame
            {
                CardId = cardDetails.Id,
                Name = cardDetails.Name,
                TypeLine = cardDetails.TypeLine,
                ManaCost = cardDetails.ManaCost,
                ImageUrl = cardDetails.ImageUrl,
                Power = cardDetails.Power,
                Toughness = cardDetails.Toughness,
                IsTapped = false,
                HasSummoningSickness = cardDetails.TypeLine?.Contains("Creature") ?? false
            };

            s.Zones[battlefieldKey].Add(cardOnBattlefield);
            log.Information("✅ {CardName} placed on battlefield for player {PlayerId}", cardDetails.Name, playerId);
            log.Information("✨ END PlayCardAsync");
            return s;
        }

        public bool IsSpellPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Main || s.CurrentPhase == Phase.Combat;

        public async Task ValidateInstantAsync(GameSession s, string playerId, string cardId)
        {
            var log = Log.ForContext("Method", nameof(ValidateInstantAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId);
            log.Information("💥 BEGIN ValidateInstantAsync");

            if (!IsSpellPhase(s, playerId))
            {
                log.Warning("Not in a valid phase to cast instant");
                throw new InvalidOperationException("Cannot cast instant now");
            }

            var handKey = $"{playerId}_hand";
            if (!s.Zones.ContainsKey(handKey) || !s.Zones[handKey].Any(c => c.CardId == cardId))
            {
                log.Warning("Instant {CardId} not in hand", cardId);
                throw new InvalidOperationException("Card not in hand");
            }

            var card = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Instant not found");

            if (card.TypeLine == null || !card.TypeLine.Contains("Instant", StringComparison.OrdinalIgnoreCase))
            {
                log.Warning("Card {CardId} is not an instant", cardId);
                throw new InvalidOperationException("Card is not an instant");
            }

            if (!CanPayManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, card.ManaCost))
            {
                log.Warning("Insufficient mana for instant {CardName}", card.Name);
                throw new InvalidOperationException("Insufficient mana to cast instant");
            }

            log.Information("✅ Instant {CardName} validated for play", card.Name);
            log.Information("💥 END ValidateInstantAsync");
        }

        public async Task<GameSession> CastInstantAsync(GameSession s, string playerId, string cardId, string? targetId)
        {
            var log = Log.ForContext("Method", nameof(CastInstantAsync))
                         .ForContext("PlayerId", playerId)
                         .ForContext("CardId", cardId)
                         .ForContext("TargetId", targetId);
            log.Information("⚡ BEGIN CastInstantAsync");

            await ValidateInstantAsync(s, playerId, cardId);

            var handKey = $"{playerId}_hand";
            var battlefieldKey = $"{playerId}_battlefield";

            var cardInHand = s.Zones[handKey].FirstOrDefault(c => c.CardId == cardId)
                ?? throw new InvalidOperationException("Card not in hand during instant cast");

            var cardDetails = await _cardClient.GetCardByIdAsync(cardId)
                ?? throw new KeyNotFoundException("Instant details missing");

            DeductManaCost(s.Players.First(p => p.PlayerId == playerId).ManaPool, cardDetails.ManaCost);
            log.Information("Mana deducted for instant {CardName}", cardDetails.Name);

            s.Zones[handKey].Remove(cardInHand);

            var cardOnBattlefield = new CardInGame
            {
                CardId = cardDetails.Id,
                Name = cardDetails.Name,
                TypeLine = cardDetails.TypeLine,
                ManaCost = cardDetails.ManaCost,
                IsTapped = false,
                HasSummoningSickness = false
            };
            s.Zones[battlefieldKey].Add(cardOnBattlefield);

            log.Information("✅ Instant {CardName} cast by {PlayerId} targeting {TargetId}", cardDetails.Name, playerId, targetId ?? "None");
            log.Information("⚡ END CastInstantAsync");
            return s;
        }

        #endregion

        // ==========================================      ==========================================

        #region Combat / Attack / Block

        public GameSession StartCombatPhase(GameSession s, string playerId)
        {
            var log = Log.ForContext("Method", nameof(StartCombatPhase)).ForContext("PlayerId", playerId);
            log.Information("🗡️ BEGIN StartCombatPhase");

            if (s.CurrentPhase != Phase.Main || s.ActivePlayerId != playerId)
            {
                log.Warning("Cannot start combat — wrong phase or inactive player");
                throw new InvalidOperationException("Cannot start combat now");
            }

            s.CurrentPhase = Phase.Combat;
            log.Information("✅ Combat phase started for player {PlayerId}", playerId);
            log.Information("🗡️ END StartCombatPhase");
            return s;
        }

        public bool IsCombatPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Combat && s.ActivePlayerId == playerId;

        public async Task ValidateAttackAsync(GameSession s, string playerId, List<string> attackers)
        {
            var log = Log.ForContext("Method", nameof(ValidateAttackAsync))
                         .ForContext("PlayerId", playerId);
            log.Information("⚔️ BEGIN ValidateAttackAsync (Attackers={Count})", attackers.Count);

            if (!IsCombatPhase(s, playerId))
            {
                log.Warning("Not in combat phase for attacks");
                throw new InvalidOperationException("Not in Combat phase");
            }

            var battlefieldKey = $"{playerId}_battlefield";
            foreach (var cardId in attackers)
            {
                var card = s.Zones[battlefieldKey].FirstOrDefault(c => c.CardId == cardId)
                    ?? throw new InvalidOperationException($"Card {cardId} not on battlefield");

                if (card.IsTapped || card.HasSummoningSickness)
                    throw new InvalidOperationException($"Card {cardId} cannot attack (tapped or summoning sickness)");

                var details = await _cardClient.GetCardByIdAsync(cardId)
                    ?? throw new KeyNotFoundException($"Card metadata for {cardId} not found");

                if (details.TypeLine == null || !details.TypeLine.Contains("Creature", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Card {cardId} is not a creature");

                log.Debug("Validated attacker {CardName}", details.Name);
            }

            log.Information("✅ All attackers validated for player {PlayerId}", playerId);
            log.Information("⚔️ END ValidateAttackAsync");
        }

        public async Task<GameSession> ResolveCombatAsync(GameSession s, string playerId, List<string> attackers, Dictionary<string, string> blockers)
        {
            var log = Log.ForContext("Method", nameof(ResolveCombatAsync))
                         .ForContext("PlayerId", playerId);
            log.Information("🩸 BEGIN ResolveCombatAsync");

            await ValidateAttackAsync(s, playerId, attackers);

            var opponentId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            var opponentBattlefieldKey = $"{opponentId}_battlefield";
            var myBattlefieldKey = $"{playerId}_battlefield";
            var graveyardKey = $"{playerId}_graveyard";
            var opponentGraveyardKey = $"{opponentId}_graveyard";

            // Blocked combat pairs
            foreach (var kvp in blockers)
            {
                var attackerId = kvp.Key;
                var blockerId = kvp.Value;

                var attacker = s.Zones[myBattlefieldKey].FirstOrDefault(c => c.CardId == attackerId);
                var blocker = s.Zones[opponentBattlefieldKey].FirstOrDefault(c => c.CardId == blockerId);

                if (attacker == null || blocker == null)
                {
                    log.Warning("Invalid attacker/blocker pair ({Attacker}, {Blocker})", attackerId, blockerId);
                    continue;
                }

                var attackerDetails = await _cardClient.GetCardByIdAsync(attackerId);
                var blockerDetails = await _cardClient.GetCardByIdAsync(blockerId);

                if ((attackerDetails?.Power ?? 0) >= (blockerDetails?.Toughness ?? int.MaxValue))
                {
                    s.Zones[opponentBattlefieldKey].Remove(blocker);
                    s.Zones[opponentGraveyardKey].Add(blocker);
                    log.Information("☠️ Blocker {BlockerName} destroyed by attacker {AttackerName}", blocker?.Name, attacker?.Name);
                }

                if ((blockerDetails?.Power ?? 0) >= (attackerDetails?.Toughness ?? int.MaxValue))
                {
                    s.Zones[myBattlefieldKey].Remove(attacker);
                    s.Zones[graveyardKey].Add(attacker);
                    log.Information("☠️ Attacker {AttackerName} destroyed by blocker {BlockerName}", attacker?.Name, blocker?.Name);
                }
            }

            // Unblocked damage
            var unblocked = attackers.Where(a => !blockers.ContainsKey(a)).ToList();
            var opponent = s.Players.First(p => p.PlayerId == opponentId);
            foreach (var attackerId in unblocked)
            {
                var attackerDetails = await _cardClient.GetCardByIdAsync(attackerId);
                var dmg = attackerDetails?.Power ?? 0;
                opponent.LifeTotal -= dmg;
                log.Information("💥 {AttackerName} dealt {Damage} damage to {OpponentId} (Life now {Life})",
                    attackerDetails?.Name, dmg, opponentId, opponent.LifeTotal);
            }

            log.Information("🩸 END ResolveCombatAsync — Opponent life: {Life}", opponent.LifeTotal);
            return s;
        }

        public GameSession ResolveCombatPhase(GameSession s, string playerId)
        {
            var log = Log.ForContext("Method", nameof(ResolveCombatPhase))
                         .ForContext("PlayerId", playerId);
            log.Information("🏁 BEGIN ResolveCombatPhase");

            if (!IsCombatPhase(s, playerId))
                throw new InvalidOperationException("Not in Combat phase");

            s.CurrentPhase = Phase.Main;
            log.Information("✅ Combat phase resolved; phase returned to MAIN");
            log.Information("🏁 END ResolveCombatPhase");
            return s;
        }

        public bool IsBlockPhase(GameSession session, string playerId)
    => session.CurrentPhase == Phase.Combat && session.ActivePlayerId != playerId;

        public async Task ValidateBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            var log = Log.ForContext("Method", nameof(ValidateBlockAsync)).ForContext("PlayerId", playerId);
            log.Information("🛡️ BEGIN ValidateBlockAsync Count={Count}", blockers?.Count ?? 0);

            if (!IsBlockPhase(session, playerId))
            {
                log.Warning("Not in block phase");
                throw new InvalidOperationException("Not in Block phase");
            }

            var battlefieldKey = $"{playerId}_battlefield";

            foreach (var blockerId in blockers.Values)
            {
                var blockerCard = session.Zones[battlefieldKey].FirstOrDefault(c => c.CardId == blockerId);
                if (blockerCard == null)
                {
                    log.Warning("Blocker {BlockerId} not on battlefield", blockerId);
                    throw new InvalidOperationException($"Blocker {blockerId} not on battlefield");
                }
                if (blockerCard.IsTapped)
                {
                    log.Warning("Blocker {BlockerId} is tapped", blockerId);
                    throw new InvalidOperationException($"Blocker {blockerId} is tapped");
                }

                var details = await _cardClient.GetCardByIdAsync(blockerId)
                    ?? throw new KeyNotFoundException($"Blocker metadata {blockerId} not found");

                if (details.TypeLine == null || !details.TypeLine.Contains("Creature", StringComparison.OrdinalIgnoreCase))
                {
                    log.Warning("Blocker {BlockerId} is not a creature", blockerId);
                    throw new InvalidOperationException($"Blocker {blockerId} is not a creature");
                }

                log.Debug("Blocker validated: {Blocker}", blockerId);
            }

            log.Information("✅ ValidateBlockAsync completed for player {PlayerId}", playerId);
        }

        public Task<GameSession> ResolveBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers)
        {
            var log = Log.ForContext("Method", nameof(ResolveBlockAsync)).ForContext("PlayerId", playerId);
            log.Information("[ResolveBlockAsync] BEGIN (noop wrapper)");
            // For now we delegate to ResolveCombatAsync at a higher level; keep wrapper to match interface.
            log.Information("[ResolveBlockAsync] END (noop wrapper)");
            return Task.FromResult(session);
        }

        #endregion

        // ==========================================      ==========================================

        #region Discard / Pre-end / end-turn 

        /// <summary>
        /// Discard cards from hand and handle simple blocker interactions.
        /// Matches IGameRulesEngine.DiscardCards(GameSession, string, List<string>, Dictionary<string,string>)
        /// </summary>
        public async Task<GameSession> DiscardCards(GameSession session, string playerId, List<string> cardsToDiscard, Dictionary<string, string> blockers)
        {
            var log = Log.ForContext("Method", nameof(DiscardCards)).ForContext("PlayerId", playerId);
            log.Information("[DiscardCards] BEGIN player={PlayerId} cardsToDiscard={Count} blockers={BlockersCount}",
                playerId, cardsToDiscard?.Count ?? 0, blockers?.Count ?? 0);

            if (session == null) throw new ArgumentNullException(nameof(session));

            // safe zone keys
            var handKey = $"{playerId}_hand";
            var myBattleKey = $"{playerId}_battlefield";
            var myGraveKey = $"{playerId}_graveyard";
            var opponentId = session.PlayerOneId == playerId ? session.PlayerTwoId : session.PlayerOneId;
            var oppBattleKey = $"{opponentId}_battlefield";
            var oppGraveKey = $"{opponentId}_graveyard";

            // ensure zone dictionaries exist before using
            if (!session.Zones.ContainsKey(handKey)) session.Zones[handKey] = new List<CardInGame>();
            if (!session.Zones.ContainsKey(myBattleKey)) session.Zones[myBattleKey] = new List<CardInGame>();
            if (!session.Zones.ContainsKey(myGraveKey)) session.Zones[myGraveKey] = new List<CardInGame>();
            if (!session.Zones.ContainsKey(oppBattleKey)) session.Zones[oppBattleKey] = new List<CardInGame>();
            if (!session.Zones.ContainsKey(oppGraveKey)) session.Zones[oppGraveKey] = new List<CardInGame>();

            // process blocker-based exchanges (defensive: check existence before remove)
            if (blockers != null)
            {
                foreach (var kvp in blockers)
                {
                    var attackerId = kvp.Key;
                    var blockerId = kvp.Value;

                    var attacker = session.Zones.TryGetValue(oppBattleKey, out var oppBattle) ? oppBattle.FirstOrDefault(c => c.CardId == attackerId) : null;
                    var blocker = session.Zones.TryGetValue(myBattleKey, out var myBattle) ? myBattle.FirstOrDefault(c => c.CardId == blockerId) : null;

                    if (attacker == null || blocker == null)
                    {
                        log.Debug("[DiscardCards] attacker or blocker not found attacker={Attacker} blocker={Blocker}", attackerId, blockerId);
                        continue;
                    }

                    var attackerDetails = await _cardClient.GetCardByIdAsync(attackerId).ConfigureAwait(false);
                    var blockerDetails = await _cardClient.GetCardByIdAsync(blockerId).ConfigureAwait(false);

                    // defensive null checks for card metadata
                    if (attackerDetails == null || blockerDetails == null)
                    {
                        log.Warning("[DiscardCards] missing metadata for attacker={Attacker} or blocker={Blocker}", attackerId, blockerId);
                    }

                    // kill blocker if attacker power >= blocker toughness
                    if ((attackerDetails?.Power ?? 0) >= (blockerDetails?.Toughness ?? int.MaxValue))
                    {
                        // Remove safely (check collection contains instance)
                        if (session.Zones[myBattleKey].Contains(blocker))
                        {
                            session.Zones[myBattleKey].Remove(blocker);
                            session.Zones[myGraveKey].Add(blocker);
                            log.Debug("[DiscardCards] Moved blocker {Blocker} to graveyard", blockerId);
                        }
                    }

                    // kill attacker if blocker power >= attacker toughness
                    if ((blockerDetails?.Power ?? 0) >= (attackerDetails?.Toughness ?? int.MaxValue))
                    {
                        if (session.Zones[oppBattleKey].Contains(attacker))
                        {
                            session.Zones[oppBattleKey].Remove(attacker);
                            session.Zones[oppGraveKey].Add(attacker);
                            log.Debug("[DiscardCards] Moved attacker {Attacker} to graveyard", attackerId);
                        }
                    }
                }
            }

            // process explicit discards from hand
            if (cardsToDiscard != null && cardsToDiscard.Count > 0)
            {
                var hand = session.Zones.TryGetValue(handKey, out var listHand) ? listHand : new List<CardInGame>();
                foreach (var discardId in cardsToDiscard)
                {
                    if (string.IsNullOrEmpty(discardId)) continue;

                    var card = hand.FirstOrDefault(c => c.CardId == discardId);
                    if (card == null)
                    {
                        log.Warning("[DiscardCards] Card {CardId} not found in hand for player={PlayerId}", discardId, playerId);
                        continue;
                    }

                    // safe remove
                    if (session.Zones[handKey].Contains(card))
                    {
                        session.Zones[handKey].Remove(card);
                    }
                    // ensure graveyard exists and add
                    if (!session.Zones.ContainsKey(myGraveKey))
                        session.Zones[myGraveKey] = new List<CardInGame>();

                    session.Zones[myGraveKey].Add(card);
                    log.Debug("[DiscardCards] Discarded {CardId} from {PlayerId}'s hand", discardId, playerId);
                }
            }

            log.Information("[DiscardCards] END player={PlayerId} handCount={Hand} graveyardCount={Grave}", playerId,
                session.Zones.TryGetValue(handKey, out var h) ? h.Count : 0,
                session.Zones.TryGetValue(myGraveKey, out var g) ? g.Count : 0);

            return session;
        }

        public bool IsPreEndPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.Main && s.ActivePlayerId == playerId;

        public GameSession PreEndCheck(GameSession s, string playerId)
        {
            var log = Log.ForContext("Method", nameof(PreEndCheck)).ForContext("PlayerId", playerId);
            log.Information("[PreEndCheck] BEGIN");
            if (!IsPreEndPhase(s, playerId))
            {
                log.Warning("[PreEndCheck] Not in pre-end phase");
                throw new InvalidOperationException("Not in Pre-End phase");
            }

            // Place for pre-end triggers; currently no automatic effects.
            log.Information("[PreEndCheck] Completed (no automatic effects)");
            return s;
        }

        public bool IsEndPhase(GameSession s, string playerId)
            => s.CurrentPhase == Phase.End && s.ActivePlayerId == playerId;

        public GameSession EndTurn(GameSession s, string playerId)
        {
            var log = Log.ForContext("Method", nameof(EndTurn)).ForContext("PlayerId", playerId);
            log.Information("🔚 BEGIN EndTurn for player {PlayerId} Phase={Phase}", playerId, s.CurrentPhase);

            if (!IsEndPhase(s, playerId))
            {
                log.Warning("EndTurn called while not in End phase - forcing to End would be caller responsibility");
                throw new InvalidOperationException("Not in End phase");
            }

            // Switch active player and reset per-turn flags
            var previousActive = s.ActivePlayerId;
            s.ActivePlayerId = s.PlayerOneId == playerId ? s.PlayerTwoId : s.PlayerOneId;
            s.CurrentPhase = Phase.Draw;

            foreach (var p in s.Players)
            {
                p.HasDrawnThisTurn = false;
                p.LandsPlayedThisTurn = 0;
            }

            log.Information("[EndTurn] playerEnding={PlayerId} newActivePlayer={ActivePlayer} newPhase={Phase}",
                playerId, s.ActivePlayerId, s.CurrentPhase);

            // Mana burn: deduct life for unused mana
            var player = s.Players.First(p => p.PlayerId == playerId);
            int unusedMana = player.ManaPool.Values.Sum();
            if (unusedMana > 0)
            {
                player.LifeTotal -= unusedMana;
                _logger.LogInformation("[ManaBurn] Player {PlayerId} loses {Unused} life from unused mana. New life: {Life}", playerId, unusedMana, player.LifeTotal);
            }

            if (player.LifeTotal <= 0)
            {
                _logger.LogInformation("[GameOver] Player {PlayerId} has 0 or less life. Ending game.", playerId);

                // Marque la phase comme finie
                s.CurrentPhase = Phase.End;
            }

            // Reset mana pool to 0
            foreach (var key in player.ManaPool.Keys.ToList())
            {
                player.ManaPool[key] = 0;
            }

            // Dump state after turn end
            StateDump(s, $"EndTurn after {previousActive} finished");

            return s;
        }

        #endregion

        // ==========================================      ==========================================

        #region Load/Save + EndGame + Mana helpers + Utilities

        /// <summary>
        /// Saves session using repository and logs a debug entry.
        /// </summary>
        public async Task<GameSession> LoadSessionAsync(string sessionId)
        {
            var log = Log.ForContext("Method", nameof(LoadSessionAsync)).ForContext("SessionId", sessionId);
            log.Debug("[LoadSessionAsync] Loading session {SessionId}", sessionId);
            var session = await _gameSessionRepository.GetByIdAsync(sessionId)
                ?? throw new InvalidOperationException($"Session {sessionId} not found");
            log.Debug("[LoadSessionAsync] Loaded session {SessionId}", sessionId);
            return session;
        }

        public async Task SaveSessionAsync(GameSession session)
        {
            var log = Log.ForContext("Method", nameof(SaveSessionAsync)).ForContext("SessionId", session?.Id);
            if (session == null) throw new ArgumentNullException(nameof(session));
            log.Debug("[SaveSessionAsync] Persisting session {SessionId}", session.Id);
            await _gameSessionRepository.UpdateAsync(session);
            log.Debug("[SaveSessionAsync] Persist completed for {SessionId}", session.Id);
        }

        public EndGameDto? CheckEndGame(GameSession session)
        {
            var log = Log.ForContext("Method", nameof(CheckEndGame));
            log.Debug("[CheckEndGame] BEGIN");

            var loser = session.Players.FirstOrDefault(p => p.LifeTotal <= 0);
            if (loser != null)
            {
                var winner = session.Players.First(p => p.PlayerId != loser.PlayerId);
                var dto = new EndGameDto
                {
                    WinnerId = winner.PlayerId,
                    Reason = $"Player {loser.PlayerId} has 0 life."
                };
                log.Information("[CheckEndGame] Found winner {Winner} because {Reason}", dto.WinnerId, dto.Reason);
                return dto;
            }

            foreach (var player in session.Players)
            {
                var libraryKey = $"{player.PlayerId}_library";
                if (session.Zones.TryGetValue(libraryKey, out var lib) && lib.Count == 0 && !player.HasDrawnThisTurn)
                {
                    var winner = session.Players.First(p => p.PlayerId != player.PlayerId);
                    var dto = new EndGameDto
                    {
                        WinnerId = winner.PlayerId,
                        Reason = $"Player {player.PlayerId} has no cards to draw."
                    };
                    log.Information("[CheckEndGame] Found winner {Winner} because {Reason}", dto.WinnerId, dto.Reason);
                    return dto;
                }
            }

            log.Debug("[CheckEndGame] No end condition found");
            return null;
        }

        #endregion

        // ==========================================      ==========================================

        #region Mana helpers & ChooseBestLandColor + Utilities

        // Keep a thin wrapper around existing parsing helper where possible; here we include a fallback parser
        private Dictionary<string, int> ParseManaCost(string manaCost)
        {
            // If external helper exists, use it; otherwise use the inline parser seen earlier.
            try
            {
                return ManaCostHelper.ParseManaCost(manaCost);
            }
            catch
            {
                // fallback: simple parse like earlier code
                var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (string.IsNullOrEmpty(manaCost)) return dict;
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
                                    _ => token
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
                                _ => "Colorless"
                            };
                            dict[color] = dict.GetValueOrDefault(color, 0) + 1;
                            i++;
                        }
                    }
                }
                return dict;
            }
        }

        private bool CanPayManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            var log = Log.ForContext("Method", nameof(CanPayManaCost));
            log.Debug("[CanPayManaCost] Checking cost {ManaCost} against pool {@Pool}", manaCost, manaPool);

            if (string.IsNullOrEmpty(manaCost)) return true;

            var required = ParseManaCost(manaCost);

            // Check colored mana first
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                var color = kvp.Key;
                var need = kvp.Value;
                if (manaPool.GetValueOrDefault(color, 0) < need)
                {
                    // allow substitution if some lands produce multiple colors (handled by heuristic elsewhere)
                    log.Debug("[CanPayManaCost] Missing {Need} of {Color} — pool has {Have}", need, color, manaPool.GetValueOrDefault(color, 0));
                    return false;
                }
            }

            // Check generic (total)
            var generic = required.GetValueOrDefault("GENERIC", 0);
            var totalAvailable = manaPool.Values.Sum();
            var coloredUsed = required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)).Sum(k => k.Value);
            var canPayGeneric = totalAvailable - coloredUsed >= generic;
            log.Debug("[CanPayManaCost] generic check: totalAvailable={Total} coloredUsed={Colored} genericReq={Generic} result={Result}",
                totalAvailable, coloredUsed, generic, canPayGeneric);
            return canPayGeneric;
        }

        private void DeductManaCost(Dictionary<string, int> manaPool, string manaCost)
        {
            var log = Log.ForContext("Method", nameof(DeductManaCost));
            log.Debug("[DeductManaCost] Deducting {ManaCost} from pool {@Pool}", manaCost, manaPool);

            if (string.IsNullOrEmpty(manaCost)) return;

            var required = ParseManaCost(manaCost);

            // Deduct colored mana
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                var color = kvp.Key;
                var need = kvp.Value;
                if (!manaPool.ContainsKey(color) || manaPool[color] < need)
                {
                    log.Error("[DeductManaCost] Insufficient {Color} mana. Pool: {@Pool}, Need: {Need}", color, manaPool, need);
                    throw new InvalidOperationException($"Insufficient {color} mana");
                }
                manaPool[color] -= need;
            }

            // Deduct generic across the pool in deterministic order
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
            {
                log.Error("[DeductManaCost] Not enough generic mana after deduction. Remaining generic: {Generic}", generic);
                throw new InvalidOperationException("Insufficient generic mana");
            }

            log.Debug("[DeductManaCost] Deduction complete. New pool {@Pool}", manaPool);
        }

        /// <summary>
        /// Choose best land color heuristic — used by AI to select which color to add when playing a multi-color land.
        /// </summary>
        public string ChooseBestLandColor(GameSession session, string playerId, List<string>? availableColors = null)
        {
            var log = Log.ForContext("Method", nameof(ChooseBestLandColor)).ForContext("PlayerId", playerId);
            log.Debug("[ChooseBestLandColor] Begin heuristic (availableColors={Available})", availableColors);

            var handKey = $"{playerId}_hand";
            var hand = session.Zones.ContainsKey(handKey) ? session.Zones[handKey] : new List<CardInGame>();
            var player = session.Players.First(p => p.PlayerId == playerId);
            var pool = player.ManaPool ?? new Dictionary<string, int>();

            var colorScores = new Dictionary<string, int>
    {
        {"White", 0},
        {"Blue", 0},
        {"Black", 0},
        {"Red", 0},
        {"Green", 0}
    };

            foreach (var cardInGame in hand)
            {
                if (string.IsNullOrEmpty(cardInGame.CardId)) continue;
                var details = _cardClient.GetCardByIdAsync(cardInGame.CardId).GetAwaiter().GetResult();
                if (details == null || string.IsNullOrEmpty(details.ManaCost)) continue;
                var required = ManaCostHelper.ParseManaCost(details.ManaCost);
                foreach (var req in required)
                    if (colorScores.ContainsKey(req.Key)) colorScores[req.Key] += req.Value;
            }

            foreach (var color in colorScores.Keys.ToList())
                if (!pool.TryGetValue(color, out int poolAmount) || poolAmount == 0)
                    colorScores[color] = (int)(colorScores[color] * 1.1);

            var ordered = colorScores.OrderByDescending(k => k.Value).Select(k => k.Key).ToList();

            if (availableColors != null && availableColors.Any())
            {
                var availSet = new HashSet<string>(availableColors, StringComparer.OrdinalIgnoreCase);
                var pick = ordered.FirstOrDefault(c => availSet.Contains(c));
                if (!string.IsNullOrEmpty(pick)) return pick;
                // fallback: return first available normalized
                return availableColors.First();
            }

            // If no availableColors constraint, return best scored color if any score > 0, otherwise null
            var best = ordered.FirstOrDefault();
            if (!string.IsNullOrEmpty(best) && colorScores[best] > 0)
                return best;

            // No clear preference: return null to allow caller to handle fallback
            log.Debug("[ChooseBestLandColor] No strong preference found, returning null");
            return null;

        }

        private List<string> Shuffle(List<string> cards)
        {
            var rng = new Random();
            return cards.OrderBy(_ => rng.Next()).ToList();
        }

        private int TotalManaCost(string manaCost)
        {
            var parsed = ParseManaCost(manaCost);
            return parsed.Values.Sum();
        }

        #endregion

        // ==========================================      ==========================================

        #region State Dump utility

        /// <summary>
        /// Logs a compact but informative snapshot of the current game session:
        /// players, life totals, active player, phase, and counts of zones.
        /// Call this at end of major steps to help debugging.
        /// </summary>
        private void StateDump(GameSession s, string context)
        {
            try
            {
                var log = Log.ForContext("Context", context).ForContext("SessionId", s?.Id ?? "null");
                if (s == null)
                {
                    log.Warning("[StateDump] Session is null for context {Context}", context);
                    return;
                }

                var players = s.Players.Select(p => new
                {
                    p.PlayerId,
                    p.LifeTotal,
                    p.HasDrawnThisTurn,
                    p.LandsPlayedThisTurn,
                    ManaPool = p.ManaPool != null ? string.Join(",", p.ManaPool.Select(kv => $"{kv.Key}={kv.Value}")) : "N/A"
                }).ToList();

                var zoneCounts = s.Zones.ToDictionary(z => z.Key, z => z.Value?.Count ?? 0);

                log.Information("[StateDump:{Context}] Phase={Phase} ActivePlayer={Active} Players={Players} ZoneCounts={ZoneCounts}",
                    context,
                    s.CurrentPhase,
                    s.ActivePlayerId,
                    players,
                    zoneCounts);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[StateDump] Failed to dump state for context {Context}", context);
            }
        }

        #endregion

    } 
} 

