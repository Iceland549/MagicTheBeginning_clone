using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GameMicroservice.Infrastructure
{
    public class CardHttpClient : ICardClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public CardHttpClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _client.BaseAddress = new Uri(config["CardMicroserviceBaseUrl"] ?? "http://card:5002");
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
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get token: {response.StatusCode}, {error}");
            }

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

        public async Task<CardDto?> GetCardByNameAsync(string cardName)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/cards/{cardName}");
            Console.WriteLine($"Response for card {cardName}: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Card data: {content}");
            return JsonSerializer.Deserialize<CardDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}