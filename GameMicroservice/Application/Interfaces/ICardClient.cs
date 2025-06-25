using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;
namespace GameMicroservice.Infrastructure
{
    public interface ICardClient
    {
        Task<CardDto?> GetCardByIdAsync(string cardId);
    }

    public class CardDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ManaCost { get; set; } = null!;
        public string TypeLine { get; set; } = null!;
        public string OracleText { get; set; } = null!;
        public int Cmc { get; set; }
        public int? Power { get; set; }
        public int? Toughness { get; set; }
        public List<string> Abilities { get; set; } = new();
        public bool IsTapped { get; set; }
        public string? ImageUrl { get; set; }
    }
}