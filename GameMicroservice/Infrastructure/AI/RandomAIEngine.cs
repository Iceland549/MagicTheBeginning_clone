using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
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
            Console.WriteLine($"[AI] Player {aiState.PlayerId} Hand: {string.Join(", ", hand.Select(c => $"{c.CardName}({c.ManaCost ?? "free"})"))}");

            // 1️⃣ Priorité : jouer un terrain si possible et permis
            if (aiState.LandsPlayedThisTurn < 1)
            {
                var land = hand.FirstOrDefault(c => c.TypeLine?.IndexOf("Land", StringComparison.OrdinalIgnoreCase) >= 0);
                if (land != null)
                {
                    Console.WriteLine($"[AI] Choisit de jouer un terrain : {land.CardName}");
                    return new PlayerActionDto
                    {
                        PlayerId = aiState.PlayerId,
                        Type = ActionType.PlayLand,
                        CardName = land.CardName
                    };
                }
            }

            // 2️⃣ Cherche les cartes jouables selon le mana disponible
            var playable = GetPlayableCards(aiState, hand).ToList();
            Console.WriteLine($"[AI] Cartes jouables : {string.Join(", ", playable.Select(c => c.CardName))}");

            if (playable.Any())
            {
                // Priorise les créatures, puis le coût de mana le plus bas
                var best = playable
                    .OrderBy(c => GetManaValue(c.ManaCost))
                    .ThenBy(c => c.TypeLine?.Contains("Creature") == true ? 0 : 1)
                    .First();

                Console.WriteLine($"[AI] Joue : {best.CardName} (coût {best.ManaCost})");
                return new PlayerActionDto
                {
                    PlayerId = aiState.PlayerId,
                    Type = ActionType.PlayCard,
                    CardName = best.CardName
                };
            }

            // 3️⃣ Si aucune carte n’est jouable → fin de tour
            Console.WriteLine("[AI] Aucune carte jouable, fin du tour");
            return new PlayerActionDto
            {
                PlayerId = aiState.PlayerId,
                Type = ActionType.EndTurn
            };
        }

        // Filtre les cartes jouables selon le mana disponible
        private IEnumerable<CardInGame> GetPlayableCards(PlayerState state, List<CardInGame> hand)
        {
            return hand.Where(card =>
                card.TypeLine?.IndexOf("Land", StringComparison.OrdinalIgnoreCase) < 0 && // pas des terrains
                CanAfford(state.ManaPool, card.ManaCost ?? ""));
        }

        // Vérifie si le joueur peut payer le coût
        private bool CanAfford(Dictionary<string, int> manaPool, string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return true;

            var required = ParseManaCost(manaCost);

            // Vérifie les coûts colorés
            foreach (var kvp in required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)))
            {
                if (manaPool.GetValueOrDefault(kvp.Key) < kvp.Value)
                    return false;
            }

            // Vérifie le coût générique
            var generic = required.GetValueOrDefault("GENERIC", 0);
            var totalAvailable = manaPool.Values.Sum();
            var coloredUsed = required.Where(k => !string.Equals(k.Key, "GENERIC", StringComparison.OrdinalIgnoreCase)).Sum(k => k.Value);

            return totalAvailable - coloredUsed >= generic;
        }

        // Parse le coût de mana dans le même format que GameRulesEngine
        private Dictionary<string, int> ParseManaCost(string manaCost)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(manaCost))
                return dict;

            // Format avec accolades : {G}{G}{2}, etc.
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

                        if (int.TryParse(token, out int num))
                            dict["GENERIC"] = dict.GetValueOrDefault("GENERIC", 0) + num;
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
                                _ => "Colorless"
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
                // Format simple : "2GG", "3G", etc.
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

        // Donne la valeur totale d’un coût de mana (pour prioriser les cartes)
        private int GetManaValue(string? manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return 0;

            var parsed = ParseManaCost(manaCost);
            return parsed.Values.Sum();
        }
    }
}
