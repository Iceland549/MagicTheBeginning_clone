using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;
using MongoDB.Driver;

namespace CardMicroservice.Infrastructure.Persistence.Repositories
{
    public class MongoCardRepository : ICardRepository
    {
        private readonly IMongoCollection<CardDto> _col;

        public MongoCardRepository(IConfiguration cfg)
        {
            var client = new MongoClient(cfg["Mongo:ConnectionString"]);
            var db = client.GetDatabase(cfg["Mongo:Database"]);
            _col = db.GetCollection<CardDto>(cfg["Mongo:CardCollection"]);
        }

        public Task<List<CardDto>> GetAllAsync() =>
            _col.Find(_ => true).ToListAsync();

        public async Task<CardDto?> GetByNameAsync(string name)
        {
            return await _col.Find(c => c.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync(); // Returns a CardDto if found, otherwise null
        }


        public Task AddAsync(CardDto card) =>
            _col.InsertOneAsync(card);
    }
}