using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GameMicroservice.Infrastructure
{
    public class DeckHttpClient : IDeckClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public DeckHttpClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _client.BaseAddress = new Uri(_config["DeckMicroserviceBaseUrl"] ?? "http://deck:5003");
        }

        private async Task<string> GetServiceTokenAsync()
        {
            // Si token en cache encore valide
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var authUrl = _config["AuthMicroserviceBaseUrl"] + "/api/service-auth/token";
            var request = new
            {
                ClientId = "game-service",
                ClientSecret = "SuperSecretGame" // ⚠️ mettre en secrets/env en prod
            };

            var response = await _client.PostAsJsonAsync(authUrl, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>();
            if (result == null)
                throw new InvalidOperationException("AuthMicroservice did not return a token");

            _cachedToken = result.AccessToken;
            _tokenExpiry = result.ExpiresAt.AddMinutes(-5); 
            return _cachedToken;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = await GetServiceTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<DeckDto?> GetDeckByIdAsync(string deckId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/decks/{deckId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DeckDto>();
        }

        public async Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/decks/{ownerId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DeckDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<DeckDto>();
        }

        public async Task<List<DeckDto>> GetAllDecksAsync()
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync("/api/decks/all");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DeckDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<DeckDto>();
        }
    }

    public class ServiceTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
