using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GameMicroservice.Infrastructure.AI
{
    /// <summary>
    /// Simple random AI: picks the first valid action.
    /// </summary>
    public class RandomAIEngine : IAIEngine
    {
        public PlayerActionDto? DecideNextAction(PlayerState state, GameSession session, List<CardInGame> hand)
        {
            var playerId = state.PlayerId;
            var battlefieldKey = $"{playerId}_battlefield";
            var battlefield = session.Zones.ContainsKey(battlefieldKey) ? session.Zones[battlefieldKey] : new List<CardInGame>();

            // If can draw
            if (!state.HasDrawnThisTurn)
                return new PlayerActionDto { Type = ActionType.Draw };

            // If can play a land
            if (battlefield.Count < state.LandsPlayedThisTurn + 1 && hand.Any())
                return new PlayerActionDto { Type = ActionType.PlayLand, CardId = hand.First().CardId };

            // Otherwise end turn
            return new PlayerActionDto { Type = ActionType.EndTurn };
        }
    }
}