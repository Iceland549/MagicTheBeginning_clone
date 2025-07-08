using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DeckMicroservice.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Represents an individual card within a deck.
    /// </summary>
    public class DeckCard
    {
        [BsonElement("cardName")]
        public string CardName { get; set; } = null!;             

        [BsonElement("quantity")]
        public int Quantity { get; set; }                       // Number of copies of this card
    }
}
