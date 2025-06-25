using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Infrastructure
{
    public interface IDeckClient
    {
        Task<List<DeckDto>> GetDecksByOwnerAsync(string ownerId);
    }

    public class DeckDto
    {
        public string OwnerId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<DeckCardDto> Cards { get; set; } = new List<DeckCardDto>();
    }

    public class DeckCardDto
    {
        public string CardName { get; set; } = null!;
        public int Quantity { get; set; }
    }
}