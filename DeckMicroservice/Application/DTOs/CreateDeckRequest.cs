namespace DeckMicroservice.Application.DTOs
{
    public class CreateDeckRequest
    {
        public string OwnerId { get; set; } = null!;   // Player ID (GUID)
        public string Name { get; set; } = null!;   // Deck name
        public List<DeckCardDto> Cards { get; set; } = new();   // List of cards
    }
}