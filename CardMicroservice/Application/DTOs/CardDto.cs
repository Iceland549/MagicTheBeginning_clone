using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CardMicroservice.Application.DTOs
{
    public class CardDto
    {
        [BsonId] // utilisé par MongoDB pour son _id interne
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        //[JsonPropertyName("id")] // correspond à l’ID Scryfall
        //public string ScryfallId { get; set; } = null!;

        [BsonElement("normalizedName")]
        [JsonIgnore] 
        public string NormalizedName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("manaCost")]
        public string ManaCost { get; set; } = null!;

        [JsonPropertyName("typeLine")]
        public string TypeLine { get; set; } = null!;

        [JsonPropertyName("oracleText")]
        public string OracleText { get; set; } = null!;

        [JsonPropertyName("power")]
        public int? Power { get; set; }

        [JsonPropertyName("toughness")]
        public int? Toughness { get; set; }

        [JsonPropertyName("abilities")]
        public List<string> Abilities { get; set; } = new(); 

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; } 

        [JsonPropertyName("object")]
        public string Object { get; set; } = null!;

        [JsonPropertyName("oracleId")]
        public string OracleId { get; set; } = null!;

        [JsonPropertyName("multiverseIds")]
        public List<int> MultiverseIds { get; set; } = new();

        [JsonPropertyName("mtgoId")]
        public int? MtgoId { get; set; }

        [JsonPropertyName("mtgoFoilId")]
        public int? MtgoFoilId { get; set; }

        [JsonPropertyName("tcgplayerId")]
        public int? TcgplayerId { get; set; }

        [JsonPropertyName("cardmarketId")]
        public int? CardmarketId { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = null!;

        [JsonPropertyName("releasedAt")]
        public string ReleasedAt { get; set; } = null!;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;

        [JsonPropertyName("scryfallUri")]
        public string ScryfallUri { get; set; } = null!;

        [JsonPropertyName("layout")]
        public string Layout { get; set; } = null!;

        [JsonPropertyName("highresImage")]
        public bool HighresImage { get; set; }

        [JsonPropertyName("image_uris")]
        public Dictionary<string, string>? ImageUris { get; set; }

        [JsonPropertyName("cmc")]
        public int Cmc { get; set; } // Corrigé en int pour compatibilité

        [JsonPropertyName("colors")]
        public List<string> Colors { get; set; } = new();

        [JsonPropertyName("color_identity")]
        public List<string> ColorIdentity { get; set; } = new();

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

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

        [JsonPropertyName("setId")]
        public string SetId { get; set; } = null!;

        [JsonPropertyName("set")]
        public string Set { get; set; } = null!;

        [JsonPropertyName("setName")]
        public string SetName { get; set; } = null!;

        [JsonPropertyName("setType")]
        public string SetType { get; set; } = null!;

        [JsonPropertyName("setUri")]
        public string SetUri { get; set; } = null!;

        [JsonPropertyName("setSearchUri")]
        public string SetSearchUri { get; set; } = null!;

        [JsonPropertyName("scryfallSetUri")]
        public string ScryfallSetUri { get; set; } = null!;

        [JsonPropertyName("rulingsUri")]
        public string RulingsUri { get; set; } = null!;

        [JsonPropertyName("printsSearchUri")]
        public string PrintsSearchUri { get; set; } = null!;

        [JsonPropertyName("collectorNumber")]
        public string CollectorNumber { get; set; } = null!;

        [JsonPropertyName("digital")]
        public bool Digital { get; set; }

        [JsonPropertyName("rarity")]
        public string Rarity { get; set; } = null!;

        [JsonPropertyName("flavorText")]
        public string? FlavorText { get; set; }

        [JsonPropertyName("cardBackId")]
        public string? CardBackId { get; set; }

        [JsonPropertyName("artist")]
        public string? Artist { get; set; }

        [JsonPropertyName("artistIds")]
        public List<string> ArtistIds { get; set; } = new();

        [JsonPropertyName("illustrationId")]
        public string? IllustrationId { get; set; }

        [JsonPropertyName("borderColor")]
        public string BorderColor { get; set; } = null!;

        [JsonPropertyName("frame")]
        public string Frame { get; set; } = null!;

        [JsonPropertyName("securityStamp")]
        public string? SecurityStamp { get; set; }

        [JsonPropertyName("fullArt")]
        public bool FullArt { get; set; }

        [JsonPropertyName("textless")]
        public bool Textless { get; set; }

        [JsonPropertyName("booster")]
        public bool Booster { get; set; }

        [JsonPropertyName("storySpotlight")]
        public bool StorySpotlight { get; set; }

        [JsonPropertyName("edhrecRank")]
        public int? EdhrecRank { get; set; }

        [JsonPropertyName("pennyRank")]
        public int? PennyRank { get; set; }

        [JsonPropertyName("prices")]
        public Dictionary<string, string?> Prices { get; set; } = new();

        [JsonPropertyName("relatedUris")]
        public Dictionary<string, string> RelatedUris { get; set; } = new();

        [JsonPropertyName("purchaseUris")]
        public Dictionary<string, string> PurchaseUris { get; set; } = new();
    }
}