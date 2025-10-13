using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Infrastructure.Clients.Base;
using Microsoft.Extensions.Configuration;

namespace DeckMicroservice.Infrastructure.Clients
{
    public class CardClient : ServiceTokenClientBase, ICardClient
    {
        public CardClient(HttpClient client, IConfiguration config)
            : base(client, config, "deck-service", config["DeckServiceClientSecret"] ?? "SuperSecretDeck")
        {
            _client.BaseAddress = new Uri(config["CardMicroserviceBaseUrl"] ?? "http://card:5002");
        }

        public async Task<CardDto?> GetCardByIdAsync(string cardId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/cards/{Uri.EscapeDataString(cardId)}");
            return await DeserializeResponse<CardDto>(response);
        }

        public async Task<List<CardDto>> GetAllAsync()
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync("/api/cards");
            return await DeserializeResponse<List<CardDto>>(response) ?? new();
        }
    }
}
