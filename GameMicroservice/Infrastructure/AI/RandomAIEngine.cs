using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GameMicroservice.Infrastructure.AI
{
    public class RandomAIEngine : IAIEngine
    {
        public PlayerActionDto? DecideNextAction(PlayerState aiState, GameSession session, List<CardInGame> hand)
        {
            // 1. Si possible, jouer un terrain
            var land = hand.FirstOrDefault(c => c.TypeLine != null && c.TypeLine.Contains("Land"));
            if (land != null && aiState.LandsPlayedThisTurn < 1)
            {
                return new PlayerActionDto
                {
                    PlayerId = aiState.PlayerId,
                    Type = ActionType.PlayLand,
                    CardId = land.CardId
                };
            }

            // 2. Sinon, jouer le premier sort possible (hors terrain)
            var spell = hand.FirstOrDefault(c => c.TypeLine != null && !c.TypeLine.Contains("Land"));
            if (spell != null)
            {
                return new PlayerActionDto
                {
                    PlayerId = aiState.PlayerId,
                    Type = ActionType.PlayCard,
                    CardId = spell.CardId
                };
            }

            // 3. Sinon, finir le tour
            return new PlayerActionDto
            {
                PlayerId = aiState.PlayerId,
                Type = ActionType.EndTurn
            };
        }
    }
}
