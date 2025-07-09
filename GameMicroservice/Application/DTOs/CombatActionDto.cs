using System.Collections.Generic;

namespace GameMicroservice.Application.DTOs
{
    public class CombatActionDto
    {
        public List<string> Attackers { get; set; } = new List<string>();
        public Dictionary<string, string> Blockers { get; set; } = new Dictionary<string, string>(); // attackerId -> blockerId
    }
}
