using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System.Collections.Generic;

namespace GameMicroservice.Application.UseCases
{
    public class DiscardUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IMapper _mapper;

        public DiscardUseCase(IGameSessionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, List<string> cardsToDiscard)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            var handKey = $"{playerId}_hand";
            var graveyardKey = $"{playerId}_graveyard";
            foreach (var cardId in cardsToDiscard)
            {
                var card = session.Zones[handKey].FirstOrDefault(c => c.CardId == cardId);
                if (card != null)
                {
                    session.Zones[handKey].Remove(card);
                    session.Zones[graveyardKey].Add(card);
                }
            }
            await _repo.UpdateAsync(session);

            return new ActionResultDto
            {
                Success = true,
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
