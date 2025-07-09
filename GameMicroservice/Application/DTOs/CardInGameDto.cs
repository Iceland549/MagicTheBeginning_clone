namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// Représente une carte en jeu, avec état runtime.
    /// </summary>
    public class CardInGameDto
    {
        public string CardId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string TypeLine { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ManaCost { get; set; }
        public int? Power { get; set; }
        public int? Toughness { get; set; }
        public bool IsTapped { get; set; } = false;
        public bool HasSummoningSickness { get; set; } = true;
        public List<string> AurasAttached { get; set; } = new();
        public int PlusOneCounters { get; set; } = 0;
        public string? InstanceId { get; set; }
    }
}
