using GameMicroservice.Application.DTOs;
using Ocelot.Requester;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameMicroservice.Infrastructure
{
    public class DeckHttpClient : IDeckClient
    {
        private readonly HttpClient _client;
        private readonly string _serviceToken; // Le token de service à ajouter aux requêtes


        public DeckHttpClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _client.BaseAddress = new Uri("http://gateway:5000"); // URL de l'API Gateway Ocelot
            _serviceToken = config["ServiceAuthToken"] ?? throw new InvalidOperationException("ServiceAuthToken missing in config");

        }

        public async Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/decks/{ownerId}");

            // Ajoute l'en-tête Authorization: Bearer <token>
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);

            var response = await _client.SendAsync(request); response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DeckDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<DeckDto>();
        }
        public async Task<List<DeckDto>> GetAllDecksAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/decks/all");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DeckDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<DeckDto>();
        }
    }
}