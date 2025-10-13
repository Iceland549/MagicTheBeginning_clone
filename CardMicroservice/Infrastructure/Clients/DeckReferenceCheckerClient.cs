using System;
using System.Threading.Tasks;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Infrastructure.Clients.Base;
using Microsoft.Extensions.Configuration;

namespace CardMicroservice.Infrastructure.Clients
{
    public class DeckReferenceCheckerClient : ServiceTokenClientBase, IDeckChecker
    {
        public DeckReferenceCheckerClient(HttpClient client, IConfiguration config)
            : base(client, config, "card-service", config["CardServiceClientSecret"] ?? "SuperSecretCard")
        {
            _client.BaseAddress = new Uri(config["DeckMicroserviceBaseUrl"] ?? "http://deck:5003");
        }

        public async Task<bool> IsCardUsedInDeckAsync(string cardId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/decks/exists-card/{Uri.EscapeDataString(cardId)}");
            if (!response.IsSuccessStatusCode)
                return false;

            string body = await response.Content.ReadAsStringAsync();
            return bool.TryParse(body, out var isUsed) && isUsed;
        }
    }
}
