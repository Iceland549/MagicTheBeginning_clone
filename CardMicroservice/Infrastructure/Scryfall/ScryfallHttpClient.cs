using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Infrastructure.Scryfall
{
    // Implements IScryfallClient to fetch card data from Scryfall API.
    public class ScryfallHttpClient : IScryfallClient
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "https://api.scryfall.com/cards/named?exact=";

        public ScryfallHttpClient(HttpClient http)
        {
            _http = http;
        }

        // Fetches a card by name from the Scryfall API and returns a ScryfallCardDto.
        public async Task<ScryfallCardDto?> FetchByNameAsync(string name)
        {
            var response = await _http.GetAsync($"{BaseUrl}{Uri.EscapeDataString(name)}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Scryfall API error: Status={response.StatusCode}, Content={errorContent}");
                return null;
            }

            var content = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<ScryfallCardDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
