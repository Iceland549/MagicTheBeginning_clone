using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Infrastructure.Scryfall
{
    public class ScryfallHttpClient : IScryfallClient
    {
        private readonly HttpClient _http;

        public ScryfallHttpClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<ScryfallCardDto?> FetchByNameAsync(string name, string? set = null, string? lang = null, string? collectorNumber = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Card name cannot be null or empty.", nameof(name));

            // If set + collectorNumber provided, try direct set/number path first (fastest)
            if (!string.IsNullOrWhiteSpace(set) && !string.IsNullOrWhiteSpace(collectorNumber))
            {
                string url = $"https://api.scryfall.com/cards/{Uri.EscapeDataString(set)}/{Uri.EscapeDataString(collectorNumber)}";
                if (!string.IsNullOrWhiteSpace(lang))
                    url += $"/{Uri.EscapeDataString(lang)}";

                Console.WriteLine($"🎯 Direct fetch: {url}");
                var resp = await _http.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    try
                    {
                        var dto = JsonSerializer.Deserialize<ScryfallCardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (dto != null)
                        {
                            Console.WriteLine($"🎯 Direct fetch succeeded: set={dto.Set}, collector={dto.CollectorNumber}, lang={dto.Lang}");
                            return dto;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ JSON parse error (direct fetch): {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Direct fetch error: {resp.StatusCode}");
                }

                // If direct fetch failed to return the exact printing, fall through to search logic below
            }

            // Use advanced search when set/lang/collectorNumber are supplied or when exact match is required
            // try exact search first, then fuzzy fallback
            var exact = await FetchInternalAsync(name, set, lang, collectorNumber, useExact: true);
            if (exact != null) return exact;

            Console.WriteLine($"⚠️ Exact match not found for '{name}'. Trying fuzzy search...");
            return await FetchInternalAsync(name, set, lang, collectorNumber, useExact: false);
        }

        public async Task<ScryfallCardDto?> FetchByIdAsync(string scryfallId)
        {
            if (string.IsNullOrWhiteSpace(scryfallId))
                throw new ArgumentException("ScryfallId cannot be null or empty.", nameof(scryfallId));

            string url = $"https://api.scryfall.com/cards/{Uri.EscapeDataString(scryfallId)}";
            Console.WriteLine($"🔍 Fetch by ID: {url}");

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Scryfall API Error (by ID): {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<ScryfallCardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ JSON parse error (by ID): {ex.Message}");
                return null;
            }
        }

        private async Task<ScryfallCardDto?> FetchInternalAsync(string name, string? set, string? lang, string? collectorNumber, bool useExact)
        {
            // Build a query that will return individual printings (unique=prints) when we need to filter by printing attributes
            bool needPrintingSelection = !string.IsNullOrWhiteSpace(set) || !string.IsNullOrWhiteSpace(lang) || !string.IsNullOrWhiteSpace(collectorNumber);

            if (needPrintingSelection)
            {
                // Build search query using Scryfall search syntax
                var qParts = new List<string> { $"name:\"{name}\"" };

                if (!string.IsNullOrWhiteSpace(set)) qParts.Add($"set:{set}");
                if (!string.IsNullOrWhiteSpace(collectorNumber)) qParts.Add($"number:{collectorNumber}");
                if (!string.IsNullOrWhiteSpace(lang)) qParts.Add($"lang:{lang}");

                string q = string.Join(" ", qParts);
                string url = $"https://api.scryfall.com/cards/search?q={Uri.EscapeDataString(q)}&unique=prints";
                Console.WriteLine($"🔎 Scryfall Search URL: {url} (needPrintingSelection=true)");

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Scryfall API Error (search): {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    // Check error payload
                    if (json.Contains("\"object\":\"error\""))
                    {
                        var err = JsonSerializer.Deserialize<ScryfallError>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        Console.WriteLine($"❌ Scryfall error (search): {err?.Details}");
                        return null;
                    }

                    var result = JsonSerializer.Deserialize<ScryfallSearchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result?.Data == null || !result.Data.Any())
                        return null;

                    // Try to find the best match using set, collectorNumber, lang priorities
                    ScryfallCardDto? found = null;

                    if (!string.IsNullOrWhiteSpace(set) && !string.IsNullOrWhiteSpace(collectorNumber) && !string.IsNullOrWhiteSpace(lang))
                    {
                        found = result.Data.FirstOrDefault(d =>
                            string.Equals(d.Set, set, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(d.CollectorNumber?.Trim(), collectorNumber.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(d.Lang, lang, StringComparison.OrdinalIgnoreCase));
                    }

                    if (found == null && !string.IsNullOrWhiteSpace(set) && !string.IsNullOrWhiteSpace(collectorNumber))
                    {
                        found = result.Data.FirstOrDefault(d =>
                            string.Equals(d.Set, set, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(d.CollectorNumber?.Trim(), collectorNumber.Trim(), StringComparison.OrdinalIgnoreCase));
                    }

                    if (found == null && !string.IsNullOrWhiteSpace(set))
                    {
                        found = result.Data.FirstOrDefault(d =>
                            string.Equals(d.Set, set, StringComparison.OrdinalIgnoreCase));
                    }

                    // if lang only specified
                    if (found == null && !string.IsNullOrWhiteSpace(lang))
                    {
                        found = result.Data.FirstOrDefault(d =>
                            string.Equals(d.Lang, lang, StringComparison.OrdinalIgnoreCase));
                    }

                    // fallback to first printing returned
                    found ??= result.Data.FirstOrDefault();

                    if (found != null)
                    {
                        Console.WriteLine($"✅ Scryfall chosen printing: set={found.Set}, collector={found.CollectorNumber}, lang={found.Lang}, id={found.Id}");
                        return found;
                    }

                    return null;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"❌ JSON Deserialization Error (search): {ex.Message}");
                    return null;
                }
            }
            else
            {
                // No printing-specific selection needed. Prefer /cards/named with exact/fuzzy parameter.
                string namedParam = useExact ? "exact" : "fuzzy";
                string url = $"https://api.scryfall.com/cards/named?{namedParam}={Uri.EscapeDataString(name)}";
                Console.WriteLine($"🔎 Scryfall Request URL: {url} (needPrintingSelection=false)");

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Scryfall API Error (named): {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    if (json.Contains("\"object\":\"error\""))
                    {
                        var error = JsonSerializer.Deserialize<ScryfallError>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        Console.WriteLine($"❌ Scryfall Error (named): {error?.Details}");
                        return null;
                    }

                    return JsonSerializer.Deserialize<ScryfallCardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"❌ JSON Deserialization Error (named): {ex.Message}");
                    return null;
                }
            }
        }
    }

    public class ScryfallError
    {
        public string? Code { get; set; }
        public string? Details { get; set; }
    }

    public class ScryfallSearchResult
    {
        public List<ScryfallCardDto> Data { get; set; } = new();
    }
}