using AutoMapper;
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Infrastructure.Config;
using DeckMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeckMicroservice.Infrastructure.Repositories
{
    public class MongoDeckRepository : IDeckRepository
    {
        private readonly IMongoCollection<Deck> _col;
        private readonly ICardClient _cardClient;
        private readonly IMapper _mapper;

        public MongoDeckRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig, ICardClient cardClient, IMapper mapper)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (mongoConfig == null) throw new ArgumentNullException(nameof(mongoConfig));
            if (cardClient == null) throw new ArgumentNullException(nameof(cardClient));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            _col = db.GetCollection<Deck>(mongoConfig.Value.DeckCollection);
            _cardClient = cardClient;
            _mapper = mapper;
        }

        public async Task CreateAsync(CreateDeckRequest req)
        {
            if (req == null)
            {
                Console.WriteLine("Error: CreateAsync received null request.");
                throw new ArgumentNullException(nameof(req));
            }

            Console.WriteLine($"Received CreateAsync request for ownerId: {req.OwnerId ?? "null"}, name: {req.Name ?? "unnamed"}");
            if (!await ValidateAsync(req))
                throw new InvalidOperationException("Deck validation failed. Check validation rules for details.");

            var deck = new Deck
            {
                OwnerId = req.OwnerId,
                Name = req.Name,
                Cards = req.Cards?.Select(c => new DeckCard
                {
                    CardName = c.CardName,
                    Quantity = c.Quantity
                }).ToList() ?? new List<DeckCard>()
            };
            Console.WriteLine($"Inserting deck with {deck.Cards.Count} cards");
            await _col.InsertOneAsync(deck);
            Console.WriteLine($"Deck inserted for ownerId: {deck.OwnerId}, name: {deck.Name}");
        }

        public async Task<List<DeckDto>> GetByOwnerAsync(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                Console.WriteLine("Error: GetByOwnerAsync received null or empty ownerId.");
                throw new ArgumentNullException(nameof(ownerId));
            }

            Console.WriteLine($"Fetching decks for ownerId: {ownerId}");
            var decks = await _col.Find(d => d.OwnerId == ownerId).ToListAsync();
            return decks.Select(d => new DeckDto
            {
                Id = d.Id,
                OwnerId = d.OwnerId,
                Name = d.Name,
                Cards = d.Cards?.Select(c => new DeckCardDto
                {
                    CardName = c.CardName,
                    Quantity = c.Quantity
                }).ToList() ?? new List<DeckCardDto>()
            }).ToList();
        }

        public async Task<bool> ValidateAsync(CreateDeckRequest deck)
        {
            if (deck == null)
            {
                Console.WriteLine("Error: ValidateAsync received null deck.");
                throw new ArgumentNullException(nameof(deck));
            }

            Console.WriteLine($"Validating deck: {deck.Name ?? "unnamed"}, OwnerId: {deck.OwnerId ?? "null"}, CardCount: {deck.Cards?.Count ?? 0}");
            if (string.IsNullOrEmpty(deck.Name))
                throw new InvalidOperationException("Deck name is required");

            if (deck.Cards == null || !deck.Cards.Any())
                throw new InvalidOperationException("Deck must contain at least one card");

            var total = deck.Cards.Sum(c => c.Quantity);
            Console.WriteLine($"Total cards: {total}");
            if (total < 60)
                throw new InvalidOperationException($"Deck must have at least 60 cards, found {total}");

            int landCount = 0;
            foreach (var card in deck.Cards)
            {
                if (card == null || string.IsNullOrEmpty(card.CardName))
                {
                    Console.WriteLine("Error: Invalid card in deck (null or empty CardName).");
                    throw new InvalidOperationException("Invalid card: CardName is required.");
                }

                Console.WriteLine($"Fetching card details for: {card.CardName}, Quantity: {card.Quantity}");
                try
                {
                    var cardDetails = await _cardClient.GetCardByIdAsync(card.CardName);
                    if (cardDetails == null)
                    {
                        Console.WriteLine($"Card not found: {card.CardName}");
                        throw new InvalidOperationException($"Card {card.CardName} not found in database");
                    }
                    Console.WriteLine($"Card found: {card.CardName}, TypeLine: {cardDetails.TypeLine ?? "null"}");

                    // Vérifier si la carte est un terrain (gérer les cas où TypeLine est null ou contient "Land")
                    bool isLand = !string.IsNullOrEmpty(cardDetails.TypeLine) && cardDetails.TypeLine.Contains("Land", StringComparison.OrdinalIgnoreCase);
                    if (card.Quantity > 4 && !isLand)
                    {
                        Console.WriteLine($"Validation failed: Card {card.CardName} exceeds maximum of 4 copies (Quantity: {card.Quantity})");
                        throw new InvalidOperationException($"Card {card.CardName} exceeds maximum of 4 copies");
                    }

                    if (isLand)
                        landCount += card.Quantity;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP error fetching card {card.CardName}: {ex.Message}");
                    throw new InvalidOperationException($"Failed to fetch card {card.CardName}: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error fetching card {card.CardName}: {ex}");
                    throw;
                }
            }

            Console.WriteLine($"Total lands: {landCount}");
            if (landCount < 20)
                throw new InvalidOperationException($"Deck must contain at least 20 lands, found {landCount}");

            Console.WriteLine("Deck validation successful");
            return true;
        }
    }
}