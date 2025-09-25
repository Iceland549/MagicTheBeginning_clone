using AutoMapper;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Domain;
using System.Threading.Tasks;

namespace GameMicroservice.Application.UseCases
{
    public class PassPhaseUseCase
    {
        private readonly IGameSessionRepository _repo;
        private readonly IGameRulesEngine _engine;
        private readonly IMapper _mapper;

        public PassPhaseUseCase(IGameSessionRepository repo, IGameRulesEngine engine, IMapper mapper)
        {
            _repo = repo;
            _engine = engine;
            _mapper = mapper;
        }

        public async Task<ActionResultDto> ExecuteAsync(string sessionId, string playerId, ActionType nextPhase)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
                return new ActionResultDto { Success = false, Message = "Session introuvable" };

            switch (nextPhase)
            {
                case ActionType.PassToMain:
                    if (session.CurrentPhase != Phase.Main)
                    {
                        session.CurrentPhase = Phase.Main;
                    }
                    break;

                case ActionType.PassToCombat:
                    if (_engine.IsSpellPhase(session, playerId))
                        session = _engine.StartCombatPhase(session, playerId);
                    break;

                case ActionType.PreEnd:
                    if (_engine.IsCombatPhase(session, playerId))
                        session = _engine.ResolveCombatPhase(session, playerId);
                    break;

                case ActionType.EndTurn:
                    if (_engine.IsEndPhase(session, playerId))
                        session = _engine.EndTurn(session, playerId);
                    break;

                default:
                    return new ActionResultDto { Success = false, Message = "Action inconnue" };
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
