using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Clients.Base;
using Microsoft.Extensions.Configuration;

namespace GameMicroservice.Infrastructure.Clients
{
    public class DeckClient : ServiceTokenClientBase, IDeckClient
    {
        public DeckClient(HttpClient client, IConfiguration config)
            : base(client, config, "game-service", "SuperSecretGame")
        {
            _client.BaseAddress = new Uri(config["DeckMicroserviceBaseUrl"] ?? "http://deck:5003");
        }

        public async Task<DeckDto?> GetDeckByIdAsync(string deckId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/decks/{Uri.EscapeDataString(deckId)}");
            return await DeserializeResponse<DeckDto>(response);
        }

        public async Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId)
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync($"/api/decks/owner/{Uri.EscapeDataString(ownerId)}");
            return await DeserializeResponse<List<DeckDto>>(response) ?? new();
        }

        public async Task<List<DeckDto>> GetAllDecksAsync()
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync("/api/decks/all");
            return await DeserializeResponse<List<DeckDto>>(response) ?? new();
        }
    }
}
