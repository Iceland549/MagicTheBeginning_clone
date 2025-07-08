using System.Text.Json.Serialization;

namespace DeckMicroservice.Application.DTOs
{
    public class CardDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type_line")]
        public string TypeLine { get; set; } = string.Empty;

        [JsonPropertyName("mana_cost")]
        public string ManaCost { get; set; } = string.Empty;

        [JsonPropertyName("cmc")]
        public int Cmc { get; set; }
    }
}