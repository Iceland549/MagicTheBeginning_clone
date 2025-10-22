using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class TapLandUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<TapLandUseCase> _logger;
        private readonly IMapper _mapper;

        public TapLandUseCase(IGameRulesEngine engine, ILogger<TapLandUseCase> logger, IMapper mapper)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, string cardId)
        {
            _logger.LogInformation("[TapLandUseCase] START — SessionId={SessionId}, PlayerId={PlayerId}, CardId={CardId}", sessionId, playerId, cardId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("[TapLandUseCase] Session not found.");
                return new ActionResultDto { Success = false, Message = "Session not found." };
            }

            try
            {
                if (session.CurrentPhase != Phase.Main || session.ActivePlayerId != playerId)
                    throw new InvalidOperationException("Cannot tap land now.");

                session = await _engine.TapLandAsync(session, playerId, cardId);
                await _engine.SaveSessionAsync(session);
                _logger.LogInformation("[TapLandUseCase] Land tapped successfully.");
                return new ActionResultDto { Success = true, Message = "Land tapped", GameState = _mapper.Map<GameSessionDto>(session) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TapLandUseCase] Error tapping land {CardId}.", cardId);
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}