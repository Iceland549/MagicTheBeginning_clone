using System.Text.Json.Serialization;

namespace CardMicroservice.Infrastructure.Scryfall
{
    // Represents the raw data structure of a Scryfall API card response.
    public class ScryfallCardDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type_line")]
        public string TypeLine { get; set; } = string.Empty;

        [JsonPropertyName("mana_cost")]
        public string ManaCost { get; set; } = string.Empty;

        [JsonPropertyName("oracle_text")]
        public string OracleText { get; set; } = string.Empty;

        [JsonPropertyName("cmc")]
        public float Cmc { get; set; }

        [JsonPropertyName("power")]
        public string? Power { get; set; }

        [JsonPropertyName("toughness")]
        public string? Toughness { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("set")]
        public string Set { get; set; } = string.Empty;

        [JsonPropertyName("set_name")]
        public string SetName { get; set; } = string.Empty;

        [JsonPropertyName("rarity")]
        public string Rarity { get; set; } = string.Empty;

        [JsonPropertyName("collector_number")]
        public string CollectorNumber { get; set; } = string.Empty;

        // Represents the image_uris object from Scryfall API responses.
        public class ImageUris
        {
            [JsonPropertyName("small")]
            public string Small { get; set; } = string.Empty; // Small image URL
            [JsonPropertyName("normal")]
            public string Normal { get; set; } = string.Empty; // Normal image URL
            [JsonPropertyName("large")]
            public string Large { get; set; } = string.Empty; // Large image URL
            [JsonPropertyName("png")]
            public string Png { get; set; } = string.Empty; // PNG image URL
            [JsonPropertyName("art_crop")]
            public string ArtCrop { get; set; } = string.Empty; // Art crop image URL
        }

    }
}