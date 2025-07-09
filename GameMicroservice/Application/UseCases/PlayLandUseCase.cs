using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;

namespace GameMicroservice.Application.UseCases
{
    public class PlayLandUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public PlayLandUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo;
            _engine = engine;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, string cardId)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            if (!_engine.IsLandPhase(session, playerId))
                return new ActionResultDto { Success = false, Message = "Pas la phase de terrain" };

            await _engine.ValidatePlayLandAsync(session, playerId, cardId);
            session = _engine.PlayLand(session, playerId, cardId);
            await _repo.UpdateAsync(session);

            return new ActionResultDto
            {
                Success = true,
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
