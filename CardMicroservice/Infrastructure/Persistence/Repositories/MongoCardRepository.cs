using AutoMapper;
using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;
using CardMicroservice.Infrastructure.Config;
using CardMicroservice.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace CardMicroservice.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository MongoDB gérant les opérations CRUD sur les cartes.
    /// Intègre la logique de normalisation à l'import et des filtres précis pour la recherche.
    /// </summary>
    public class MongoCardRepository : ICardRepository
    {
        private readonly IMongoCollection<CardDto> _col;
        private readonly IMapper _mapper;

        public MongoCardRepository(IMongoDatabase db, IOptions<MongoDbConfig> mongoConfig, IMapper mapper)
        {
            _col = db.GetCollection<CardDto>(mongoConfig.Value.CardCollection);
            _mapper = mapper;

            // Création d’un index composé pour accélérer les recherches
            var indexKeys = Builders<CardDto>.IndexKeys
                .Ascending(c => c.NormalizedName)
                .Ascending(c => c.Set)
                .Ascending(c => c.Lang)
                .Ascending(c => c.CollectorNumber);

            var indexModel = new CreateIndexModel<CardDto>(indexKeys);
            try
            {
                _col.Indexes.CreateOne(indexModel);
                Console.WriteLine("[MongoRepo] Index sur (NormalizedName, Set, Lang, CollectorNumber) créé ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MongoRepo] Échec création index : {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère toutes les cartes en base.
        /// </summary>
        public async Task<List<CardDto>> GetAllAsync()
        {
            var entities = await _col.Find(_ => true).ToListAsync(); // _col est IMongoCollection<CardEntity>
            var dtos = entities.Select(e => _mapper.Map<CardDto>(e)).ToList();
            return dtos;
        }

        /// <summary>
        /// Récupère une carte à partir de son ID unique (ScryfallId ou Mongo _id).
        /// </summary>
        public async Task<CardDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var filter = Builders<CardDto>.Filter.Eq(c => c.Id, id);
            var card = await _col.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine(card != null
                ? $"[MongoRepo] ✅ Carte trouvée : {card.Name}"
                : $"[MongoRepo] ❌ Carte non trouvée pour ID = {id}");

            return card;

        }
        /// <summary>
        /// Récupère une carte par nom, set, langue et collectorNumber.
        /// Sert notamment pour les imports depuis Scryfall.
        /// </summary>
        public async Task<CardDto?> GetByNameAsync(string name, string? set = null, string? lang = null, string? collectorNumber = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var normalized = NameNormalizer.Normalize(name);
            var builder = Builders<CardDto>.Filter;
            var filter = builder.Eq(c => c.NormalizedName, normalized);

            if (!string.IsNullOrWhiteSpace(set))
                filter &= builder.Regex(c => c.Set, new BsonRegularExpression($"^{Regex.Escape(set)}$", "i"));

            if (!string.IsNullOrWhiteSpace(lang))
                filter &= builder.Regex(c => c.Lang, new BsonRegularExpression($"^{Regex.Escape(lang)}$", "i"));

            if (!string.IsNullOrWhiteSpace(collectorNumber))
                filter &= builder.Regex(c => c.CollectorNumber, new BsonRegularExpression($"^{Regex.Escape(collectorNumber)}$", "i"));

            var result = await _col.Find(filter).FirstOrDefaultAsync();
            Console.WriteLine($"[MongoRepo] Recherche par nom='{name}', set='{set}', lang='{lang}', collector='{collectorNumber}' => {(result != null ? "✅ trouvée" : "❌ absente")}");
            return result;
        }

        /// <summary>
        /// Ajoute une carte à la base après normalisation.
        /// Utilisé lors de l'import depuis Scryfall.
        /// </summary>
        public async Task AddAsync(CardDto card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            card.NormalizedName = NameNormalizer.Normalize(card.Name);

            // Évite les doublons ScryfallId / Id
            var existing = await _col.Find(Builders<CardDto>.Filter.Eq(c => c.Id, card.Id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                Console.WriteLine($"[MongoRepo] ⚠️ Carte déjà existante : {card.Name} ({card.Id}) — skip insert");
                return;
            }

            try
            {
                await _col.InsertOneAsync(card);
                Console.WriteLine($"[MongoRepo] ✅ Carte insérée : {card.Name}");
            }
            catch (MongoWriteException mwx)
            {
                Console.WriteLine($"[MongoRepo] ⚠️ Erreur d'écriture Mongo : {mwx.Message}");
                throw;
            }
        }


        /// <summary>
        /// Supprime une carte à partir de son ID.
        /// </summary>
        public async Task<bool> DeleteByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var filter = Builders<CardDto>.Filter.Eq(c => c.Id, id);
            var result = await _col.DeleteOneAsync(filter);

            Console.WriteLine(result.DeletedCount > 0
                ? $"[MongoRepo] 🗑️ Carte supprimée ID={id}"
                : $"[MongoRepo] ❌ Aucune carte trouvée à supprimer pour ID={id}");

            return result.DeletedCount > 0;
        }
    }
}
