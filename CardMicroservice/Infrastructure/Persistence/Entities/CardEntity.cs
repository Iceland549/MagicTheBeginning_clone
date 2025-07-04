using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CardMicroservice.Infrastructure.Persistence.Entities
{
    public class CardEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("mana_cost")]
        public string ManaCost { get; set; } = null!;

        [BsonElement("type_line")]
        public string TypeLine { get; set; } = null!;

        [BsonElement("oracle_text")]
        public string OracleText { get; set; } = null!;

        [BsonElement("power")]
        public int? Power { get; set; }

        [BsonElement("toughness")]
        public int? Toughness { get; set; }

        [BsonElement("abilities")]
        public List<string> Abilities { get; set; } = new();

        [BsonElement("image_url")]
        public string? ImageUrl { get; set; }

        [BsonElement("object")]
        public string Object { get; set; } = null!;

        [BsonElement("oracle_id")]
        public string OracleId { get; set; } = null!;

        [BsonElement("multiverse_ids")]
        public List<int> MultiverseIds { get; set; } = new();

        [BsonElement("mtgo_id")]
        public int? MtgoId { get; set; }

        [BsonElement("mtgo_foil_id")]
        public int? MtgoFoilId { get; set; }

        [BsonElement("tcgplayer_id")]
        public int? TcgplayerId { get; set; }

        [BsonElement("cardmarket_id")]
        public int? CardmarketId { get; set; }

        [BsonElement("lang")]
        public string Language { get; set; } = null!;

        [BsonElement("released_at")]
        public string ReleasedAt { get; set; } = null!;

        [BsonElement("uri")]
        public string Uri { get; set; } = null!;

        [BsonElement("scryfall_uri")]
        public string ScryfallUri { get; set; } = null!;

        [BsonElement("layout")]
        public string Layout { get; set; } = null!;

        [BsonElement("highres_image")]
        public bool HighresImage { get; set; }

        [BsonElement("image_uris")]
        public Dictionary<string, string>? ImageUris { get; set; }

        [BsonElement("cmc")]
        public int Cmc { get; set; }

        [BsonElement("colors")]
        public List<string> Colors { get; set; } = new();

        [BsonElement("color_identity")]
        public List<string> ColorIdentity { get; set; } = new();

        [BsonElement("keywords")]
        public List<string> Keywords { get; set; } = new();

        [BsonElement("legalities")]
        public Dictionary<string, string> Legalities { get; set; } = new();

        [BsonElement("games")]
        public List<string> Games { get; set; } = new();

        [BsonElement("reserved")]
        public bool Reserved { get; set; }

        [BsonElement("foil")]
        public bool Foil { get; set; }

        [BsonElement("nonfoil")]
        public bool Nonfoil { get; set; }

        [BsonElement("finishes")]
        public List<string> Finishes { get; set; } = new();

        [BsonElement("oversized")]
        public bool Oversized { get; set; }

        [BsonElement("promo")]
        public bool Promo { get; set; }

        [BsonElement("reprint")]
        public bool Reprint { get; set; }

        [BsonElement("variation")]
        public bool Variation { get; set; }

        [BsonElement("set_id")]
        public string SetId { get; set; } = null!;

        [BsonElement("set")]
        public string Set { get; set; } = null!;

        [BsonElement("set_name")]
        public string SetName { get; set; } = null!;

        [BsonElement("set_type")]
        public string SetType { get; set; } = null!;

        [BsonElement("set_uri")]
        public string SetUri { get; set; } = null!;

        [BsonElement("set_search_uri")]
        public string SetSearchUri { get; set; } = null!;

        [BsonElement("scryfall_set_uri")]
        public string ScryfallSetUri { get; set; } = null!;

        [BsonElement("rulings_uri")]
        public string RulingsUri { get; set; } = null!;

        [BsonElement("prints_search_uri")]
        public string PrintsSearchUri { get; set; } = null!;

        [BsonElement("collector_number")]
        public string CollectorNumber { get; set; } = null!;

        [BsonElement("digital")]
        public bool Digital { get; set; }

        [BsonElement("rarity")]
        public string Rarity { get; set; } = null!;

        [BsonElement("flavor_text")]
        public string? FlavorText { get; set; }

        [BsonElement("card_back_id")]
        public string? CardBackId { get; set; }

        [BsonElement("artist")]
        public string? Artist { get; set; }

        [BsonElement("artist_ids")]
        public List<string> ArtistIds { get; set; } = new();

        [BsonElement("illustration_id")]
        public string? IllustrationId { get; set; }

        [BsonElement("border_color")]
        public string BorderColor { get; set; } = null!;

        [BsonElement("frame")]
        public string Frame { get; set; } = null!;

        [BsonElement("security_stamp")]
        public string? SecurityStamp { get; set; }

        [BsonElement("full_art")]
        public bool FullArt { get; set; }

        [BsonElement("textless")]
        public bool Textless { get; set; }

        [BsonElement("booster")]
        public bool Booster { get; set; }

        [BsonElement("story_spotlight")]
        public bool StorySpotlight { get; set; }

        [BsonElement("edhrec_rank")]
        public int? EdhrecRank { get; set; }

        [BsonElement("penny_rank")]
        public int? PennyRank { get; set; }

        [BsonElement("prices")]
        public Dictionary<string, string?> Prices { get; set; } = new();

        [BsonElement("related_uris")]
        public Dictionary<string, string> RelatedUris { get; set; } = new();

        [BsonElement("purchase_uris")]
        public Dictionary<string, string> PurchaseUris { get; set; } = new();
    }
}