using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class DrawCardUseCase
    {
        private readonly ILogger<DrawCardUseCase> _logger;
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public DrawCardUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper, ILogger<DrawCardUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId)
        {
            _logger.LogInformation("[DrawCardUseCase] START");

            var session = await _engine.LoadSessionAsync(sessionId);

            var libraryKey = $"{playerId}_library";
            var handKey = $"{playerId}_hand";

            _logger.LogDebug("[DrawCardUseCase] Library count BEFORE: {Count}",
                session.Zones[libraryKey].Count);
            _logger.LogDebug("[DrawCardUseCase] Hand count BEFORE: {Count}",
                session.Zones[handKey].Count);

            var cardToDraw = session.Zones[libraryKey].FirstOrDefault();
            _logger.LogDebug("[DrawCardUseCase] Card to draw: CardId={CardId}, Name={Name}, TypeLine={TypeLine}",
                cardToDraw?.CardId, cardToDraw?.Name ?? "NULL", cardToDraw?.TypeLine ?? "NULL");

            session = await _engine.DrawStepAsync(session, playerId);

            var drawnCard = session.Zones[handKey].LastOrDefault();
            _logger.LogInformation("[DrawCardUseCase] Card drawn: CardId={CardId}, Name={Name}, TypeLine={TypeLine}, ManaCost={ManaCost}",
                drawnCard?.CardId, drawnCard?.Name ?? "NULL", drawnCard?.TypeLine ?? "NULL", drawnCard?.ManaCost ?? "NULL");

            _logger.LogDebug("[DrawCardUseCase] Library count AFTER: {Count}",
                session.Zones[libraryKey].Count);
            _logger.LogDebug("[DrawCardUseCase] Hand count AFTER: {Count}",
                session.Zones[handKey].Count);
            await _engine.SaveSessionAsync(session);

            return new ActionResultDto { /* ... */ };
        }
    }
}
