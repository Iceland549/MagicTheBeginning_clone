using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class DrawCardUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public DrawCardUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo;
            _engine = engine;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId)
        {
            Console.WriteLine($"[DrawCardUseCase] Start: SessionId={sessionId}, Player={playerId}");

            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
            {
                Console.WriteLine($"[DrawCardUseCase] ERREUR: Session introuvable pour Id={sessionId}");
                return new ActionResultDto { Success = false, Message = "Session introuvable" };
            }

            Console.WriteLine($"[DrawCardUseCase] Session trouvée: Phase={session.CurrentPhase}, ActivePlayer={session.ActivePlayerId}, Turn={session.TurnNumber}");

            if (session.ActivePlayerId != playerId)
                return new ActionResultDto { Success = false, Message = "Ce n’est pas le tour du joueur" };

            var handKey = $"{playerId}_hand";
            var libraryKey = $"{playerId}_library";

            if (!session.Zones.ContainsKey(handKey) || !session.Zones.ContainsKey(libraryKey))
                return new ActionResultDto { Success = false, Message = "Zones de jeu incohérentes" };

            var hand = session.Zones[handKey];
            var deck = session.Zones[libraryKey];

            Console.WriteLine($"[DrawCardUseCase] DeckCount={deck.Count}, HandCount={hand.Count}");

            if (deck.Count == 0)
                return new ActionResultDto { Success = false, Message = "Le deck est vide, pas de pioche possible" };

            if (session.TurnNumber == 1 && session.ActivePlayerId == playerId)
            {
                session.CurrentPhase = Phase.Main;
                await _repo.UpdateAsync(session);
                Console.WriteLine($"[DrawCardUseCase] Premier tour: pas de pioche pour Player={playerId}");

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

            Console.WriteLine($"[DrawCardUseCase] Carte piochée: {drawnCard.CardId}, Phase={session.CurrentPhase}, DeckRestant={deck.Count}, Main={hand.Count}");

            //if (session.CurrentPhase != Phase.Draw)
            //{
            //    session.CurrentPhase = Phase.Draw;
            //}

            session = _engine.DrawStep(session, playerId);
            await _repo.UpdateAsync(session);

            Console.WriteLine($"[DrawCardUseCase] Après DrawStep: Phase={session.CurrentPhase}, Hand={hand.Count}, Deck={deck.Count}");

            return new ActionResultDto
            {
                Success = true,
                Message = $"Carte piochée: {drawnCard}",
                GameState = _mapper.Map<GameSessionDto>(session)
            };
        }
    }
}
