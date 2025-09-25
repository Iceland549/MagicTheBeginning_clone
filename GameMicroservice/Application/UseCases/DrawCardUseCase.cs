using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class DrawCardUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IMapper _mapper;

        public DrawCardUseCase(IGameSessionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            // Par exemple, si ActivePlayerId est la clé pour savoir quel joueur agit
            if (session.ActivePlayerId != playerId)
                return new ActionResultDto { Success = false, Message = "Ce n’est pas le tour du joueur" };

            var handKey = $"{playerId}_hand";
            var deckKey = "deck";

            if (!session.Zones.ContainsKey(handKey) || !session.Zones.ContainsKey(deckKey))
                return new ActionResultDto { Success = false, Message = "Zones de jeu incohérentes" };

            var hand = session.Zones[handKey];
            var deck = session.Zones[deckKey];

            if (deck.Count == 0)
                return new ActionResultDto { Success = false, Message = "Le deck est vide, pas de pioche possible" };

            // Si 1er tour et joueur débutant, pas de pioche
            if (session.TurnNumber == 1 && session.ActivePlayerId == playerId)
            {
                return new ActionResultDto
                {
                    Success = true,
                    Message = "Pas de pioche au premier tour du joueur commençant",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }

            var drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);

            await _repo.UpdateAsync(session);

            return new ActionResultDto
            {
                Success = true,
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
