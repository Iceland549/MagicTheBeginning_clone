using MongoDB.Bson.Serialization.Attributes;

namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    public class CardInGame
    {
        [BsonElement("cardName")]
        public string CardName { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }

        [BsonElement("manaCost")]
        public string? ManaCost { get; set; }

        [BsonElement("typeLine")]
        public string? TypeLine { get; set; }

        [BsonElement("power")]
        public int? Power { get; set; }

        [BsonElement("toughness")]
        public int? Toughness { get; set; }

        [BsonElement("isTapped")]
        public bool IsTapped { get; set; }

        [BsonElement("hasSummoningSickness")]
        public bool HasSummoningSickness { get; set; }

        public CardInGame() { }

        public CardInGame(string cardName)
        {
            CardName = cardName;
            IsTapped = false;
            HasSummoningSickness = false;
        }
    }
}
