using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace GameMicroservice.Application.Helpers
{
    public static class ManaCostHelper
    {
        public static List<string> ExtractSymbols(string manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return new List<string>();

            // Ex : "{1}{R}{W}" → ["1", "R", "W"]
            var matches = Regex.Matches(manaCost, @"\{(.*?)\}");
            return matches.Select(m => m.Groups[1].Value).ToList();
        }
        public static Dictionary<string, int> ParseManaCost(string manaCost)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(manaCost))
                return dict;

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

        public static int ComputeTotalManaValue(string? manaCost)
        {
            if (string.IsNullOrEmpty(manaCost))
                return 0;
            var parsed = ParseManaCost(manaCost);
            return parsed.Values.Sum();
        }

        public static bool CanPayManaCost(Dictionary<string, int> manaPool, string manaCost)
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

        public static void DeductManaCost(Dictionary<string, int> manaPool, string manaCost)
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

            // Deduct generic from remaining mana
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
    }
}
