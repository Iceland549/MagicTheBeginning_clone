using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace GameMicroservice.Application.UseCases.Combat
{
    public class TapCreatureUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<TapCreatureUseCase> _logger;
        private readonly IMapper _mapper;

        public TapCreatureUseCase(
            IGameRulesEngine engine,
            ILogger<TapCreatureUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(
            string sessionId,
            string playerId,
            string cardId)
        {
            _logger.LogInformation("[TapCreature] START - Player={PlayerId}, Card={CardId}",
                playerId, cardId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                session = await _engine.TapCreatureAsync(session, playerId, cardId);
                await _engine.SaveSessionAsync(session);

                _logger.LogInformation("[TapCreature] SUCCESS - {CardId} engagée", cardId);

                return new ActionResultDto
                {
                    Success = true,
                    Message = $"Créature {cardId} engagée",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TapCreature] ERROR - {CardId}", cardId);
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}

