using MongoDB.Bson.Serialization.Attributes;

namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    public class CardInGame
    {
        [BsonElement("cardId")]
        public string CardId { get; set; } = null!;

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

        [BsonElement("canBeTapped")]
        public bool CanBeTapped { get; set; }

        [BsonElement("hasSummoningSickness")]
        public bool HasSummoningSickness { get; set; }

        [BsonElement("chosenLandColor")]
        public string? ChosenLandColor { get; set; }

        [BsonElement("availableManaColors")]
        public List<string>? AvailableManaColors { get; set; } = new();

        [BsonElement("playedThisTurn")]
        public bool PlayedThisTurn { get; set; } = false;
        public CardInGame() { }

        public CardInGame(string cardId)
        {
            CardId = cardId;
            IsTapped = false;
            HasSummoningSickness = false;
            ChosenLandColor = null;
        }
    }
}
