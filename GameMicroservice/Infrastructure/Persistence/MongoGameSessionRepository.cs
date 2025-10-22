using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Infrastructure.Persistence
{
    /// <summary>
    /// MongoDB repository for GameSession entities.
    /// Pure persistence layer — contains no game logic or rules.
    /// All gameplay validation and state management belong to GameRulesEngine.
    /// </summary>
    public class MongoGameSessionRepository : IGameSessionRepository
    {
        private readonly IMongoCollection<GameSession> _collection;
        private readonly ILogger<MongoGameSessionRepository> _logger;

        public MongoGameSessionRepository(IMongoDatabase database, ILogger<MongoGameSessionRepository> logger)
        {
            _collection = database?.GetCollection<GameSession>("GameSessions")
                ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GameSession?> GetByIdAsync(string id)
        {
            _logger.LogDebug("[MongoRepo] Fetching GameSession id={Id}", id);
            var filter = Builders<GameSession>.Filter.Eq(s => s.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<GameSession>> ListAllAsync()
        {
            _logger.LogDebug("[MongoRepo] Listing all GameSessions");
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task CreateAsync(GameSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            await _collection.InsertOneAsync(session);
            _logger.LogInformation("[MongoRepo] Created GameSession id={Id}", session.Id);
        }

        public async Task UpdateAsync(GameSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            var filter = Builders<GameSession>.Filter.Eq(s => s.Id, session.Id);
            var result = await _collection.ReplaceOneAsync(filter, session, new ReplaceOptions { IsUpsert = false });

            if (result.IsAcknowledged)
                _logger.LogDebug("[MongoRepo] Updated GameSession id={Id}", session.Id);
            else
                _logger.LogWarning("[MongoRepo] Update not acknowledged for GameSession id={Id}", session.Id);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<GameSession>.Filter.Eq(s => s.Id, id);
            var result = await _collection.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
                _logger.LogInformation("[MongoRepo] Deleted GameSession id={Id}", id);
            else
                _logger.LogWarning("[MongoRepo] GameSession id={Id} not found for deletion", id);
        }
    }
}
