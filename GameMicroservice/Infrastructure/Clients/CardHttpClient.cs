using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;


namespace GameMicroservice.Infrastructure
{
    public class CardHttpClient : ICardClient
    {
        private readonly HttpClient _client;

        public CardHttpClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("http://gateway:5000"); // URL de l'API Gateway Ocelot
        }

        public async Task<CardDto?> GetCardByIdAsync(string cardId)
        {
            var response = await _client.GetAsync($"/api/cards/{cardId}");
            Console.WriteLine($"Response for card {cardId}: {response.StatusCode}");
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