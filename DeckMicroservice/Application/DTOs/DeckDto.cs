namespace DeckMicroservice.Application.DTOs
{
    public class DeckDto
    {
        public string Id { get; set; } = null!;
        public string OwnerId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<DeckCardDto> Cards { get; set; } = new();
    }
}
