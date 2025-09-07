using GameMicroservice.Application.DTOs;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Application.Interfaces
{
    public interface IGameRulesEngine
    {
        bool HasDrawnThisTurn(GameSession s, string playerId);
        GameSession DrawStep(GameSession s, string playerId);
        bool IsLandPhase(GameSession s, string playerId);
        Task ValidatePlayLandAsync(GameSession s, string playerId, string cardId);
        GameSession PlayLand(GameSession s, string playerId, string cardId);
        GameSession OnLandfall(GameSession s, string playerId, string cardId);
        bool IsMainPhase(GameSession s, string playerId);
        Task ValidatePlayAsync(GameSession s, string playerId, string cardId);
        Task<GameSession> PlayCardAsync(GameSession s, string playerId, string cardId);
        bool IsSpellPhase(GameSession s, string playerId);
        Task ValidateInstantAsync(GameSession s, string playerId, string cardId);
        Task<GameSession> CastInstantAsync(GameSession s, string playerId, string cardId, string? targetId);
        GameSession StartCombatPhase(GameSession s, string playerId);
        bool IsCombatPhase(GameSession s, string playerId);
        Task ValidateAttackAsync(GameSession s, string playerId, List<string> attackers);
        Task<GameSession> ResolveCombatAsync(GameSession s, string playerId, List<string> attackers, Dictionary<string, string> blockers);
        GameSession ResolveCombatPhase(GameSession s, string playerId);
        bool IsPreEndPhase(GameSession s, string playerId);
        GameSession PreEndCheck(GameSession s, string playerId);
        bool IsEndPhase(GameSession s, string playerId);
        GameSession EndTurn(GameSession s, string playerId);
        Task<GameSession> LoadSessionAsync(string sessionId);
        Task SaveSessionAsync(GameSession session);
        bool IsBlockPhase(GameSession session, string playerId);
        Task ValidateBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers);
        Task<GameSession> ResolveBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers);
        Task<GameSession> DiscardCards(GameSession session, string playerId, List<string> cardsToDiscard, Dictionary<string, string> blockers);
        EndGameDto? CheckEndGame(GameSession session);

    }
}