using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;

namespace GameMicroservice.Application.UseCases
{
    public class BlockUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public BlockUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo;
            _engine = engine;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, CombatActionDto action)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            await _engine.ValidateBlockAsync(session, playerId, action.Blockers);
            session = await _engine.ResolveBlockAsync(session, playerId, action.Blockers);
            await _repo.UpdateAsync(session);

            return new ActionResultDto
            {
                Success = true,
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
