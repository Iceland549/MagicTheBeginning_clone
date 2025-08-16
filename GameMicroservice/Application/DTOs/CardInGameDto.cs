using System.Text.Json.Serialization;

namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// Représente une carte en jeu, avec état runtime.
    /// </summary>
    public class CardInGameDto
    {
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("typeLine")]
        public string TypeLine { get; set; } = null!;

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("manaCost")]
        public string? ManaCost { get; set; }

        [JsonPropertyName("power")]
        public int? Power { get; set; }

        [JsonPropertyName("toughness")]
        public int? Toughness { get; set; }

        [JsonPropertyName("isTapped")]
        public bool IsTapped { get; set; } = false;

        [JsonPropertyName("hasSummoningSickness")]
        public bool HasSummoningSickness { get; set; } = true;

        [JsonPropertyName("aurasAttached")]
        public List<string> AurasAttached { get; set; } = new();

        [JsonPropertyName("plusOneCounters")]
        public int PlusOneCounters { get; set; } = 0;

        [JsonPropertyName("instanceId")]
        public string? InstanceId { get; set; }
    }
}
