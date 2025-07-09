using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;

namespace GameMicroservice.Application.UseCases
{
    public class PassPhaseUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public PassPhaseUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo;
            _engine = engine;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, ActionType nextPhase)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            // Exemples de transitions
            if (nextPhase == ActionType.PassToCombat && _engine.IsSpellPhase(session, playerId))
                session = _engine.StartCombatPhase(session, playerId);
            else if (nextPhase == ActionType.PreEnd && _engine.IsCombatPhase(session, playerId))
                session = _engine.ResolveCombatPhase(session, playerId);
            else if (nextPhase == ActionType.EndTurn && _engine.IsEndPhase(session, playerId))
                session = _engine.EndTurn(session, playerId);

            await _repo.UpdateAsync(session);

            return new ActionResultDto
            {
                Success = true,
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
