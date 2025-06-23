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
            var response = await _http.GetAsync(BaseUrl + Uri.EscapeDataString(name));
            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();
            // Deserializes only the fields needed for your application
            return await JsonSerializer.DeserializeAsync<ScryfallCardDto>(stream);
        }
    }
}
