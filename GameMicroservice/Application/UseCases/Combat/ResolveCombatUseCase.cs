using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases.Combat
{
    public class ResolveCombatUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<ResolveCombatUseCase> _logger;
        private readonly IMapper _mapper;

        public ResolveCombatUseCase(
            IGameRulesEngine engine,
            ILogger<ResolveCombatUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId)
        {
            _logger.LogInformation("[ResolveCombat] START - Session={SessionId}", sessionId);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                session = await _engine.ResolveCombatDamageAsync(session, playerId); 
                await _engine.SaveSessionAsync(session);

                // Vérifier fin de partie
                var endGame = _engine.CheckEndGame(session);

                _logger.LogInformation("[ResolveCombat] SUCCESS - EndGame={HasEnded}",
                    endGame != null);

                return new ActionResultDto
                {
                    Success = true,
                    Message = "Combat résolu",
                    GameState = _mapper.Map<GameSessionDto>(session),
                    EndGame = endGame
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ResolveCombat] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}

