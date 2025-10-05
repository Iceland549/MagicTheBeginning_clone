namespace GameMicroservice.Application.DTOs
{
    public enum ActionType
    {
        Draw,
        PlayLand,
        PlayCard,
        CastInstant,
        PassToMain,
        PassToCombat,
        Attack,
        Block,
        Discard,
        PreEnd,
        EndTurn,
        EndGame
    }

    public class PlayerActionDto
    {
        public string PlayerId { get; set; } = null!; 

        public ActionType Type { get; set; }
        public string? CardName { get; set; }
        public List<string>? Attackers { get; set; }
        public Dictionary<string, string>? Blockers { get; set; }
        public string? TargetId { get; set; }
        public List<string>? CardsToDiscard { get; set; }
        public CombatActionDto? CombatAction { get; set; }


    }
}