namespace GameMicroservice.Application.DTOs
{
    /// <summary>
    /// Représente une carte en jeu, avec état runtime.
    /// </summary>
    public class CardInGameDto
    {
        public string CardId { get; set; } = null!;
        public bool IsTapped { get; set; } = false;
        public bool HasSummoningSickness { get; set; } = true;
        public List<string> AurasAttached { get; set; } = new();
        public int PlusOneCounters { get; set; } = 0;
        public string? InstanceId { get; set; }
    }
}
