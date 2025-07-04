using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Infrastructure.Persistence.Entities;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using DeckMicroservice.Infrastructure.Config;

namespace DeckMicroservice.Infrastructure.Persistence.Repositories
{
    public class MongoDeckRepository : IDeckRepository
    {
        private readonly IMongoCollection<Deck> _col;

        public MongoDeckRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig)
        {
            _col = db.GetCollection<Deck>(mongoConfig.Value.DeckCollection);
        }

        public async Task CreateAsync(CreateDeckRequest req)
        {
            Console.WriteLine($"Received CreateAsync request for ownerId: {req?.OwnerId ?? "null"}, name: {req?.Name ?? "unnamed"}");
            // Convert DTO to entity
            var deck = new Deck
            {
                OwnerId = req.OwnerId,
                Name = req.Name,
                Cards = req.Cards.Select(c => new DeckCard
                { CardId = c.CardName, Quantity = c.Quantity })
                    .ToList()
            };
            Console.WriteLine($"Inserting deck with {deck.Cards.Count} cards");
            await _col.InsertOneAsync(deck);
            Console.WriteLine($"Deck inserted for ownerId: {deck.OwnerId}, name: {deck.Name}");
        }

        public async Task<List<DeckDto>> GetByOwnerAsync(string ownerId)
        {
            var decks = await _col.Find(d => d.OwnerId == ownerId).ToListAsync();
            // Map entity → DTO
            return decks.Select(d => new DeckDto
            {
                Id = d.Id,
                OwnerId = d.OwnerId,
                Name = d.Name,
                Cards = d.Cards.Select(c => new DeckCardDto
                { CardName = c.CardId, Quantity = c.Quantity })
                    .ToList()
            }).ToList();
        }

        public bool Validate(CreateDeckRequest deck)
        {
            // Rules:
            // - Minimum 60 cards
            // - Maximum 4 copies of a card
            // - Minimum 20 lands (count "land" in the name)
            var total = deck.Cards.Sum(c => c.Quantity);
            if (total < 60) return false;

            if (deck.Cards.Any(c => c.Quantity > 4)) return false;

            var lands = deck.Cards
                .Where(c => c.CardName.ToLower().Contains("land"))
                .Sum(c => c.Quantity);
            if (lands < 20) return false;

            return true;
        }
    }
}
