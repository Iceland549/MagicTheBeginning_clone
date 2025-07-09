namespace GameMicroservice.Application.DTOs
{
    public class DeckDto
    {
        public string OwnerId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<DeckCardDto> Cards { get; set; } = new();
        public string Id { get; set; } = null!;

    }

}
