using System;
using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Clients.Base;
using Microsoft.Extensions.Configuration;

namespace GameMicroservice.Infrastructure.Clients
{
    public class CardClient : ServiceTokenClientBase, ICardClient
    {
        public CardClient(HttpClient client, IConfiguration config)
            : base(client, config, "game-service", "SuperSecretGame")
        {
            _client.BaseAddress = new Uri(config["CardMicroserviceBaseUrl"] ?? "http://card:5002");
        }

        public async Task<CardDto?> GetCardByIdAsync(string cardId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/cards/{Uri.EscapeDataString(cardId)}");
            return await DeserializeResponse<CardDto>(response);
        }
    }
}
