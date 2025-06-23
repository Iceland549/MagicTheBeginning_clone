namespace CardMicroservice.Application.DTOs
{
    public class CardDto
    {
        public string Id { get; set; } = null!;       // GUID Scryfall
        public string Name { get; set; } = null!;       // Nom complet
        public string ManaCost { get; set; } = null!;       // "{2}{G}"
        public string TypeLine { get; set; } = null!;       // "Creature — Elf Druid"
        public string OracleText { get; set; } = null!;       // Texte de règle
        public int Cmc { get; set; }                // Converted mana cost
        public int? Power { get; set; }                // Pour créatures
        public int? Toughness { get; set; }                // Pour créatures
        public List<string> Abilities { get; set; } = new();      // Keywords
        public bool IsTapped { get; set; }                // State de jeu
        public string? ImageUrl { get; set; }                // URL de l’image normale
    }
}