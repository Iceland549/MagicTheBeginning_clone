using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Infrastructure.Clients
{
    /// <summary>
    /// Client HTTP pour interagir avec CardMicroservice via l'API Gateway.
    /// </summary>
    public class CardHttpClient : ICardClient
    {
        private readonly HttpClient _client;

        public CardHttpClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("http://gateway:5000"); // URL de l'API Gateway
        }

        public async Task<CardDto?> GetCardByIdAsync(string cardId)
        {
            var response = await _client.GetAsync($"/api/cards/{cardId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CardDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}