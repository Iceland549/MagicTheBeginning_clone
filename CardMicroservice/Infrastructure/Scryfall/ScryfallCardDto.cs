using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CardMicroservice.Infrastructure.Scryfall
{
    public class ScryfallCardDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mana_cost")]
        public string ManaCost { get; set; } = string.Empty;

        [JsonPropertyName("type_line")]
        public string TypeLine { get; set; } = string.Empty;

        [JsonPropertyName("oracle_text")]
        public string OracleText { get; set; } = string.Empty;

        [JsonPropertyName("power")]
        public string? Power { get; set; }

        [JsonPropertyName("toughness")]
        public string? Toughness { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("image_uris")]
        public ImageUris? ImageUrisData { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("oracle_id")]
        public string OracleId { get; set; } = string.Empty;

        [JsonPropertyName("multiverse_ids")]
        public List<int> MultiverseIds { get; set; } = new();

        [JsonPropertyName("mtgo_id")]
        public int? MtgoId { get; set; }

        [JsonPropertyName("mtgo_foil_id")]
        public int? MtgoFoilId { get; set; }

        [JsonPropertyName("tcgplayer_id")]
        public int? TcgplayerId { get; set; }

        [JsonPropertyName("cardmarket_id")]
        public int? CardmarketId { get; set; }

        [JsonPropertyName("lang")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("released_at")]
        public string ReleasedAt { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("scryfall_uri")]
        public string ScryfallUri { get; set; } = string.Empty;

        [JsonPropertyName("layout")]
        public string Layout { get; set; } = string.Empty;

        [JsonPropertyName("highres_image")]
        public bool HighresImage { get; set; }

        [JsonPropertyName("cmc")]
        public float Cmc { get; set; }

        [JsonPropertyName("colors")]
        public List<string> Colors { get; set; } = new();

        [JsonPropertyName("color_identity")]
        public List<string> ColorIdentity { get; set; } = new();

        [JsonPropertyName("legalities")]
        public Dictionary<string, string> Legalities { get; set; } = new();

        [JsonPropertyName("games")]
        public List<string> Games { get; set; } = new();

        [JsonPropertyName("reserved")]
        public bool Reserved { get; set; }

        [JsonPropertyName("foil")]
        public bool Foil { get; set; }

        [JsonPropertyName("nonfoil")]
        public bool Nonfoil { get; set; }

        [JsonPropertyName("finishes")]
        public List<string> Finishes { get; set; } = new();

        [JsonPropertyName("oversized")]
        public bool Oversized { get; set; }

        [JsonPropertyName("promo")]
        public bool Promo { get; set; }

        [JsonPropertyName("reprint")]
        public bool Reprint { get; set; }

        [JsonPropertyName("variation")]
        public bool Variation { get; set; }

        [JsonPropertyName("set_id")]
        public string SetId { get; set; } = string.Empty;

        [JsonPropertyName("set")]
        public string Set { get; set; } = string.Empty;

        [JsonPropertyName("set_name")]
        public string SetName { get; set; } = string.Empty;

        [JsonPropertyName("set_type")]
        public string SetType { get; set; } = string.Empty;

        [JsonPropertyName("set_uri")]
        public string SetUri { get; set; } = string.Empty;

        [JsonPropertyName("set_search_uri")]
        public string SetSearchUri { get; set; } = string.Empty;

        [JsonPropertyName("scryfall_set_uri")]
        public string ScryfallSetUri { get; set; } = string.Empty;

        [JsonPropertyName("rulings_uri")]
        public string RulingsUri { get; set; } = string.Empty;

        [JsonPropertyName("prints_search_uri")]
        public string PrintsSearchUri { get; set; } = string.Empty;

        [JsonPropertyName("collector_number")]
        public string CollectorNumber { get; set; } = string.Empty;

        [JsonPropertyName("digital")]
        public bool Digital { get; set; }

        [JsonPropertyName("rarity")]
        public string Rarity { get; set; } = string.Empty;

        [JsonPropertyName("flavor_text")]
        public string? FlavorText { get; set; }

        [JsonPropertyName("card_back_id")]
        public string? CardBackId { get; set; }

        [JsonPropertyName("artist")]
        public string? Artist { get; set; }

        [JsonPropertyName("artist_ids")]
        public List<string> ArtistIds { get; set; } = new();

        [JsonPropertyName("illustration_id")]
        public string? IllustrationId { get; set; }

        [JsonPropertyName("border_color")]
        public string BorderColor { get; set; } = string.Empty;

        [JsonPropertyName("frame")]
        public string Frame { get; set; } = string.Empty;

        [JsonPropertyName("security_stamp")]
        public string? SecurityStamp { get; set; }

        [JsonPropertyName("full_art")]
        public bool FullArt { get; set; }

        [JsonPropertyName("textless")]
        public bool Textless { get; set; }

        [JsonPropertyName("booster")]
        public bool Booster { get; set; }

        [JsonPropertyName("story_spotlight")]
        public bool StorySpotlight { get; set; }

        [JsonPropertyName("edhrec_rank")]
        public int? EdhrecRank { get; set; }

        [JsonPropertyName("penny_rank")]
        public int? PennyRank { get; set; }

        [JsonPropertyName("prices")]
        public Dictionary<string, string?> Prices { get; set; } = new();

        [JsonPropertyName("related_uris")]
        public Dictionary<string, string> RelatedUris { get; set; } = new();

        [JsonPropertyName("purchase_uris")]
        public Dictionary<string, string> PurchaseUris { get; set; } = new();

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