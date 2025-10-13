using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DeckMicroservice.Infrastructure.Clients.Base 
{
    /// <summary>
    /// Classe de base pour tous les clients inter-microservices.
    /// Gère le token d'authentification interservice et l'ajout automatique du header Authorization.
    /// </summary>
    public abstract class ServiceTokenClientBase
    {
        protected readonly HttpClient _client;
        protected readonly IConfiguration _config;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _token;
        private DateTime _tokenExpiry = DateTime.MinValue;

        protected ServiceTokenClientBase(HttpClient client, IConfiguration config, string clientId, string clientSecret)
        {
            _client = client;
            _config = config;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Récupère et met en cache le token interservice depuis AuthMicroservice.
        /// </summary>
        protected async Task<string> GetServiceTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
                return _token;

            string authUrl = _config["AuthMicroserviceBaseUrl"] ?? "http://auth:5001";
            var payload = new { ClientId = _clientId, ClientSecret = _clientSecret };

            using var authClient = new HttpClient();
            var response = await authClient.PostAsJsonAsync($"{authUrl}/api/service-auth/token", payload);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"[ServiceTokenClientBase] ❌ Échec récupération token : {response.StatusCode} - {err}");
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>();
            _token = result!.AccessToken;
            _tokenExpiry = result.ExpiresAt.AddMinutes(-5);

            Console.WriteLine($"[ServiceTokenClientBase] 🔐 Token reçu ({_clientId}), expiration {_tokenExpiry:u}");
            return _token;
        }

        /// <summary>
        /// Ajoute le header Authorization à la requête HTTP.
        /// </summary>
        protected async Task AddAuthHeaderAsync()
        {
            var token = await GetServiceTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Utilitaire générique de désérialisation de réponse HTTP.
        /// </summary>
        protected static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class ServiceTokenResponse
        {
            public string AccessToken { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
        }
    }
}
