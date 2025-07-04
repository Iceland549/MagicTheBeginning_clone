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
            return await _col.Find(c => c.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync(); // Returns a CardDto if found, otherwise null
        }


        public Task AddAsync(CardDto card) =>
            _col.InsertOneAsync(card);
    }
}