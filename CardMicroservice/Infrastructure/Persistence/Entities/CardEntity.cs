using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CardMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Représente une carte Magic dans MongoDB.
    /// </summary>
    public class CardEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;       // ObjectId Mongo

        [BsonElement("name")]
        public string Name { get; set; } = null!;     // Nom de la carte

        [BsonElement("type")]
        public string Type { get; set; } = null!;     // Type/ligne de type (Creature, Land...)

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = null!; // URL de l'image

        // ✨ Options futures
        // [BsonElement("manaCost")] public string ManaCost { get; set; }
        // [BsonElement("rarity")]   public string Rarity { get; set; }
    }
}