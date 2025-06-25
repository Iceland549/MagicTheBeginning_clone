using MongoDB.Bson.Serialization.Attributes;

namespace GameMicroservice.Infrastructure.Persistence.Entities
{
    public class CardInGame
    {
        [BsonElement("cardId")]
        public string CardId { get; set; } = null!;

        [BsonElement("isTapped")]
        public bool IsTapped { get; set; }

        [BsonElement("hasSummoningSickness")]
        public bool HasSummoningSickness { get; set; }

        public CardInGame(string cardId)
        {
            CardId = cardId;
            IsTapped = false;
            HasSummoningSickness = true;
        }
    }
}