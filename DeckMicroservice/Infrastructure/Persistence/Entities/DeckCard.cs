using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DeckMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents an individual card within a deck.
    /// </summary>
    public class DeckCard
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string CardId { get; set; } = null!;             // CardEntity id or simply the name

        [BsonElement("quantity")]
        public int Quantity { get; set; }                       // Number of copies of this card
    }
}
