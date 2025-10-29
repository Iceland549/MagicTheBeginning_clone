using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases.Combat
{
    public class DeclareBlockersUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<DeclareBlockersUseCase> _logger;
        private readonly IMapper _mapper;

        public DeclareBlockersUseCase(
            IGameRulesEngine engine,
            ILogger<DeclareBlockersUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(
            string sessionId,
            string playerId,
            Dictionary<string, string> blockers)
        {
            _logger.LogInformation("[DeclareBlockers] START - Player={PlayerId}, Blockers={Count}",
                playerId, blockers.Count);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                // Si c'est l'IA, utiliser la logique automatique
                if (playerId == "AI" || playerId == session.PlayerTwoId)
                {
                    session = await _engine.DeclareBlockersAIAsync(session, playerId);
                }
                else
                {
                    session = await _engine.DeclareBlockersAsync(session, playerId, blockers);
                }

                await _engine.SaveSessionAsync(session);

                _logger.LogInformation("[DeclareBlockers] SUCCESS - {Count} bloqueurs déclarés",
                    blockers.Count);

                return new ActionResultDto
                {
                    Success = true,
                    Message = $"{blockers.Count} bloqueurs déclarés",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeclareBlockers] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}

