using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;

namespace GameMicroservice.Application.UseCases
{
    public class EndGameUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IMapper _mapper;

        public EndGameUseCase(IGameSessionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string winnerId, string reason)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };


            return new ActionResultDto
            {
                Success = true,
                EndGame = new EndGameDto { WinnerId = winnerId, Reason = reason }
            };
        }
    }
}
