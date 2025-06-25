using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameMicroservice.Infrastructure
{
    public class DeckHttpClient : IDeckClient
    {
        private readonly HttpClient _client;

        public DeckHttpClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("http://gateway:5000"); // URL de l'API Gateway Ocelot
        }

        public async Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId)
        {
            var response = await _client.GetAsync($"/deck/api/decks/{ownerId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DeckDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<DeckDto>();
        }
    }
}