using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using CardMicroservice.Infrastructure.Config;

namespace CardMicroservice.Infrastructure.Persistence.Repositories
{
    public class MongoCardRepository : ICardRepository
    {
        private readonly IMongoCollection<CardDto> _col;

        public MongoCardRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig)
        {
            _col = db.GetCollection<CardDto>(mongoConfig.Value.CardCollection);
        }

        public async Task<List<CardDto>> GetAllAsync()
        {
            var cards = await _col.Find(_ => true).ToListAsync();
            Console.WriteLine($"[MongoRepo] Cartes récupérées : {cards.Count}");
            return cards;
        }

        public async Task<CardDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var decodedName = Uri.UnescapeDataString(name).Trim();
            return await _col.Find(c => c.Name.ToLower() == decodedName.ToLower())
                             .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Décoder le nom au cas où il viendrait encodé depuis l’URL
            var decodedName = Uri.UnescapeDataString(name).Trim();

            // ✅ Utiliser le bon type générique (CardDto) pour Builders et _col
            var filter = Builders<CardDto>.Filter.Where(
                c => c.Name.ToLower() == decodedName.ToLower()
            );

            var result = await _col.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
            {
                Console.WriteLine($"[MongoRepo] Carte supprimée : '{decodedName}'");
                return true;
            }

            Console.WriteLine($"[MongoRepo] Aucune carte trouvée avec le nom : '{decodedName}'");
            return false;
        }

        public Task AddAsync(CardDto card) =>
            _col.InsertOneAsync(card);
    }
}
