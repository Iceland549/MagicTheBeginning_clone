namespace GameMicroservice.Application.DTOs
{
    public enum ActionType
    {
        Draw,
        PlayLand,
        PlayCard,
        CastInstant,
        PassToCombat,
        Attack,
        PreEnd,
        EndTurn
    }

    public class PlayerActionDto
    {
        public ActionType Type { get; set; }
        public string? CardId { get; set; }
        public List<string>? Attackers { get; set; }
        public Dictionary<string, string>? Blockers { get; set; }
        public string? TargetId { get; set; }
    }
}