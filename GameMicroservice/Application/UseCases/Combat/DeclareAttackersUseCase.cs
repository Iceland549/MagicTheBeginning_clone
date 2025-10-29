using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace GameMicroservice.Application.UseCases.Combat
{
    public class DeclareAttackersUseCase
    {
        private readonly IGameRulesEngine _engine;
        private readonly ILogger<DeclareAttackersUseCase> _logger;
        private readonly IMapper _mapper;

        public DeclareAttackersUseCase(
            IGameRulesEngine engine,
            ILogger<DeclareAttackersUseCase> logger,
            IMapper mapper)
        {
            _engine = engine;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(
            string sessionId,
            string playerId,
            List<string> attackerIds)
        {
            _logger.LogInformation("[DeclareAttackers] START - Player={PlayerId}, Attackers={Count}",
                playerId, attackerIds.Count);

            var session = await _engine.LoadSessionAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            try
            {
                var defenderId = session.Players.FirstOrDefault(p => p.PlayerId != playerId)?.PlayerId;

                if (!string.IsNullOrEmpty(defenderId) && (defenderId == "AI" || defenderId == session.PlayerTwoId))
                {
                    // Orchestrer tout le combat (déclare attaquants -> IA choisit bloqueurs -> résolution)
                    session = await _engine.ExecuteCombatPhaseAsync(session, playerId, attackerIds);
                    await _engine.SaveSessionAsync(session);
                }
                else
                {
                    // Comportement actuel : on se contente de déclarer, le défenseur humain doit envoyer DeclareBlockers.
                    session = await _engine.DeclareAttackersAsync(session, playerId, attackerIds);
                    await _engine.SaveSessionAsync(session);
                }

                return new ActionResultDto
                {
                    Success = true,
                    Message = $"{attackerIds.Count} créatures déclarées attaquantes",
                    GameState = _mapper.Map<GameSessionDto>(session)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeclareAttackers] ERROR");
                return new ActionResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}
