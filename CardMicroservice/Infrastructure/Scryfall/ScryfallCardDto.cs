using System.Text.Json.Serialization;

namespace CardMicroservice.Infrastructure.Scryfall
{
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

        [JsonPropertyName("image_uris")]
        public ImageUris? ImageUrisData { get; set; }

        public class ImageUris
        {
            [JsonPropertyName("small")]
            public string Small { get; set; } = string.Empty;

            [JsonPropertyName("normal")]
            public string Normal { get; set; } = string.Empty;

            [JsonPropertyName("large")]
            public string Large { get; set; } = string.Empty;

            [JsonPropertyName("png")]
            public string Png { get; set; } = string.Empty;

            [JsonPropertyName("art_crop")]
            public string ArtCrop { get; set; } = string.Empty;
        }
    }
}