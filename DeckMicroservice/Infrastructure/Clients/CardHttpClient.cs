using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Infrastructure.Clients
{
    public class CardHttpClient : ICardClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private string? _serviceToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public CardHttpClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _client.BaseAddress = new Uri(_config["GatewayBaseUrl"] ?? "http://gateway:5000");
        }

        private async Task<string> GetServiceTokenAsync()
        {
            if (!string.IsNullOrEmpty(_serviceToken) && DateTime.UtcNow < _tokenExpiry)
                return _serviceToken;

            var authBaseUrl = _config["AuthMicroserviceBaseUrl"] ?? "http://auth:5001";
            var clientId = "deck-service";
            var clientSecret = _config["DeckServiceClientSecret"] ?? "SuperSecretDeck";

            var payload = new { ClientId = clientId, ClientSecret = clientSecret };
            var response = await _client.PostAsJsonAsync($"{authBaseUrl}/api/service-auth/token", payload);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to get service token: {response.StatusCode} - {err}");
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>();
            _serviceToken = result!.AccessToken;
            _tokenExpiry = result.ExpiresAt.AddMinutes(-5);
            return _serviceToken;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = await GetServiceTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<CardDto?> GetCardByIdAsync(string cardId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/cards/{Uri.EscapeDataString(cardId)}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"CardHttpClient: {cardId} -> {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class ServiceTokenResponse
        {
            public string AccessToken { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
        }
    }
}