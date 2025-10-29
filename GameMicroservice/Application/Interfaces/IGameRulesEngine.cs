using GameMicroservice.Application.DTOs;
using GameMicroservice.Infrastructure.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Application.Interfaces
{
    public interface IGameRulesEngine
    {
        Task<GameSession> TapLandAsync(GameSession s, string playerId, string cardId);
        bool HasDrawnThisTurn(GameSession s, string playerId);
        Task<GameSession> DrawStepAsync(GameSession s, string playerId);
        bool IsLandPhase(GameSession s, string playerId);
        Task ValidatePlayLandAsync(GameSession s, string playerId, string cardName);
        GameSession PlayLand(GameSession s, string playerId, string cardName);
        GameSession OnLandfall(GameSession s, string playerId, string cardName);
        bool IsMainPhase(GameSession s, string playerId);
        Task ValidatePlayAsync(GameSession s, string playerId, string cardName);
        Task<GameSession> PlayCardAsync(GameSession s, string playerId, string cardName);
        bool IsSpellPhase(GameSession s, string playerId);
        Task ValidateInstantAsync(GameSession s, string playerId, string cardName);
        Task<GameSession> CastInstantAsync(GameSession s, string playerId, string cardName, string? targetId);
        Task<GameSession> StartCombatPhaseAsync(GameSession s, string playerId);
        bool IsCombatPhase(GameSession s, string playerId);
        Task ValidateAttackAsync(GameSession s, string playerId, List<string> attackers);
        bool IsPreEndPhase(GameSession s, string playerId);
        GameSession PreEndCheck(GameSession s, string playerId);
        bool IsEndPhase(GameSession s, string playerId);
        GameSession EndTurn(GameSession s, string playerId);
        Task<GameSession> LoadSessionAsync(string sessionId);
        Task SaveSessionAsync(GameSession session);
        bool IsBlockPhase(GameSession session, string playerId);
        Task ValidateBlockAsync(GameSession session, string playerId, Dictionary<string, string> blockers);
        Task<GameSession> DiscardCards(GameSession session, string playerId, List<string> cardsToDiscard, Dictionary<string, string> blockers);
        EndGameDto? CheckEndGame(GameSession session);

        string ChooseBestLandColor(GameSession session, string playerId, List<string>? availableManaColors = null);

        Task<GameSession> TapCreatureAsync(GameSession session, string playerId, string cardId);

        Task<GameSession> DeclareAttackersAsync(
            GameSession session,
            string attackerId,
            List<string> attackerIds);

        Task<GameSession> DeclareBlockersAsync(
            GameSession session,
            string defenderId,
            Dictionary<string, string> blockers);

        Task<GameSession> DeclareBlockersAIAsync(
            GameSession session,
            string aiPlayerId);

        Task<GameSession> ResolveCombatDamageAsync(GameSession session, string playerId);

        Task<GameSession> ExecuteCombatPhaseAsync(
            GameSession session,
            string attackerId,
            List<string> attackerIds);
    }
}