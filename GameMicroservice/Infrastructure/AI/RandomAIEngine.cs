using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.Helpers;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMicroservice.Infrastructure.AI
{
    public class RandomAIEngine : IAIEngine
    {
        public PlayerActionDto? DecideNextAction(PlayerState aiState, GameSession session, List<CardInGame> hand)
        {
            Console.WriteLine($"[AI] Player {aiState.PlayerId} ManaPool: {string.Join(", ", aiState.ManaPool.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            Console.WriteLine($"[AI] Player {aiState.PlayerId} Hand: {string.Join(", ", hand.Select(c => $"{c.CardId}({c.ManaCost ?? "free"})"))}");

            // 1️⃣ Jouer un terrain si possible
            if (aiState.LandsPlayedThisTurn < 1)
            {
                var land = hand.FirstOrDefault(c => c.TypeLine?.IndexOf("Land", StringComparison.OrdinalIgnoreCase) >= 0);
                if (land != null)
                {
                    Console.WriteLine($"[AI] Choisit de jouer un terrain : {land.CardId}");
                    return new PlayerActionDto
                    {
                        PlayerId = aiState.PlayerId,
                        Type = ActionType.PlayLand,
                        CardId = land.CardId
                    };
                }
            }

            // 2️⃣ Chercher les cartes jouables selon le mana disponible
            var playable = GetPlayableCards(aiState, hand, session).ToList();
            Console.WriteLine($"[AI] Cartes jouables : {string.Join(", ", playable.Select(c => c.CardId))}");

            if (playable.Any())
            {
                // Priorise les créatures, puis le coût le plus bas
                var best = playable
                    .OrderBy(c => ManaCostHelper.ComputeTotalManaValue(c.ManaCost)) // ✅ utilisation du helper
                    .ThenBy(c => c.TypeLine?.Contains("Creature") == true ? 0 : 1)
                    .First();

                Console.WriteLine($"[AI] Joue : {best.CardId} (coût {best.ManaCost})");
                return new PlayerActionDto
                {
                    PlayerId = aiState.PlayerId,
                    Type = ActionType.PlayCard,
                    CardId = best.CardId
                };
            }

            // 3️⃣ Si aucune carte jouable → défausse la plus chère
            if (hand.Count > 7)
            {
                var toDiscard = hand
                    .OrderByDescending(c => ManaCostHelper.ComputeTotalManaValue(c.ManaCost)) // ✅ helper ici aussi
                    .First();

                Console.WriteLine($"[AI] Défausse : {toDiscard.CardId}");
                return new PlayerActionDto
                {
                    PlayerId = aiState.PlayerId,
                    Type = ActionType.Discard,
                    CardsToDiscard = new List<string> { toDiscard.CardId }
                };
            }

            // 4️⃣ Sinon, fin du tour
            Console.WriteLine("[AI] Aucune carte jouable, fin du tour");
            return new PlayerActionDto
            {
                PlayerId = aiState.PlayerId,
                Type = ActionType.EndTurn
            };
        }

        // 🔹 Filtre les cartes jouables
        // inside RandomAIEngine
        private IEnumerable<CardInGame> GetPlayableCards(PlayerState state, List<CardInGame> hand, GameSession session)
        {
            // compute currently available mana in pool
            var poolTotal = state.ManaPool?.Values.Sum() ?? 0;

            // count untapped lands on battlefield for the AI
            var battlefieldKey = $"{state.PlayerId}_battlefield";
            var untappedLandsCount = 0;
            if (session.Zones.ContainsKey(battlefieldKey))
            {
                untappedLandsCount = session.Zones[battlefieldKey]
                    .Count(c => c.TypeLine != null &&
                                c.TypeLine.IndexOf("Land", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                !c.IsTapped);
            }

            var potentialMana = poolTotal + untappedLandsCount;

            return hand.Where(card =>
                card.TypeLine?.IndexOf("Land", StringComparison.OrdinalIgnoreCase) < 0 &&
                ManaCostHelper.ComputeTotalManaValue(card.ManaCost ?? "") <= potentialMana
            );
        }


        // 🔹 Vérifie si le joueur peut payer le coût de mana
        private bool CanAfford(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return true;

            var required = ManaCostHelper.ParseManaCost(manaCost); // ✅ on réutilise le helper

            // Vérifie les coûts colorés
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                if (manaPool.GetValueOrDefault(kvp.Key) < kvp.Value)
                    return false;
            }

            // Vérifie le coût générique
            var generic = required.GetValueOrDefault("GENERIC", 0);
            var totalAvailable = manaPool.Values.Sum();
            var coloredUsed = required
                .Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase))
                .Sum(k => k.Value);

            return totalAvailable - coloredUsed >= generic;
        }
    }
}
