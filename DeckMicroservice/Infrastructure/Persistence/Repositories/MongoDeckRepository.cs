using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using DeckMicroservice.Infrastructure.Config;
using DeckMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeckMicroservice.Infrastructure.Persistence.Repositories
{
    public class MongoDeckRepository : IDeckRepository
    {
        private readonly IMongoCollection<Deck> _col;

        public MongoDeckRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig)
        {
            _col = db.GetCollection<Deck>(mongoConfig.Value.DeckCollection);
        }

        /// <summary>
        /// Ajoute un deck (le deck est déjà validé en amont).
        /// </summary>
        public async Task AddAsync(DeckDto deck)
        {
            if (deck == null)
                throw new ArgumentNullException(nameof(deck));

            var entity = new Deck
            {
                OwnerId = deck.OwnerId,
                Name = deck.Name,
                Cards = deck.Cards?.Select(c => new DeckCard
                {
                    CardId = c.CardId,
                    Quantity = c.Quantity
                }).ToList() ?? new List<DeckCard>()
            };

            await _col.InsertOneAsync(entity);
        }

        /// <summary>
        /// Récupère tous les decks d'un propriétaire donné.
        /// </summary>
        public async Task<List<DeckDto>> GetByOwnerAsync(string ownerId)
        {
            var decks = await _col.Find(d => d.OwnerId == ownerId).ToListAsync();
            return decks.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Récupère tous les decks.
        /// </summary>
        public async Task<List<DeckDto>> GetAllDecksAsync()
        {
            var decks = await _col.Find(_ => true).ToListAsync();
            return decks.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Récupère un deck par son identifiant.
        /// </summary>
        public async Task<DeckDto?> GetByIdAsync(string id)
        {
            var deck = await _col.Find(d => d.Id == id).FirstOrDefaultAsync();
            return deck == null ? null : MapToDto(deck);
        }

        /// <summary>
        /// Vérifie si une carte donnée est utilisée dans au moins un deck.
        /// </summary>
        public async Task<bool> ExistsCardAsync(string cardId)
        {
            var filter = Builders<Deck>.Filter.ElemMatch(d => d.Cards, c => c.CardId == cardId);
            var count = await _col.CountDocumentsAsync(filter);
            return count > 0;
        }

        // 🔸 Méthode utilitaire pour éviter la répétition de mapping
        private static DeckDto MapToDto(Deck deck) => new DeckDto
        {
            Id = deck.Id,
            OwnerId = deck.OwnerId,
            Name = deck.Name,
            Cards = deck.Cards.Select(c => new DeckCardDto
            {
                CardId = c.CardId,
                Quantity = c.Quantity
            }).ToList()
        };
    }
}
