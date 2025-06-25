using GameMicroservice.Application.DTOs;
using GameMicroservice.Infrastructure.Persistence.Entities;

namespace GameMicroservice.Application.Interfaces
{
    /// <summary>
    /// Contract for AI decision-making engine.
    /// Decides next PlayerActionDto based on current PlayerState and GameSession.
    /// </summary>
    public interface IAIEngine
    {
        /// <summary>
        /// Returns the next action for the AI, or null to end turn.
        /// </summary>
        PlayerActionDto? DecideNextAction(PlayerState state, GameSession session, List<CardInGame> hand);
    }
}