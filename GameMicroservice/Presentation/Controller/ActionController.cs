using Microsoft.AspNetCore.Mvc;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.UseCases;
//using GameMicroservice.Domain;
using System.Threading.Tasks;

namespace GameMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/games/{sessionId}/action")]
    public class ActionController : ControllerBase
    {
        private readonly PlayLandUseCase _playLand;
        private readonly PlayCardUseCase _playCard;
        private readonly AttackUseCase _attack;
        private readonly BlockUseCase _block;
        private readonly PassPhaseUseCase _passPhase;
        private readonly DiscardUseCase _discard;
        private readonly EndGameUseCase _endGame;

        public ActionController(
            PlayLandUseCase playLand,
            PlayCardUseCase playCard,
            AttackUseCase attack,
            BlockUseCase block,
            PassPhaseUseCase passPhase,
            DiscardUseCase discard,
            EndGameUseCase endGame)
        {
            _playLand = playLand;
            _playCard = playCard;
            _attack = attack;
            _block = block;
            _passPhase = passPhase;
            _discard = discard;
            _endGame = endGame;
        }

        [HttpPost]
        public async Task<IActionResult> PlayAction(
            [FromRoute] string sessionId,
            [FromBody] PlayerActionDto action)
        {
            ActionResultDto result = action.Type switch
            {
                ActionType.PlayLand => await _playLand.ExecuteAsync(sessionId, action.PlayerId, action.CardId!),
                ActionType.PlayCard => await _playCard.ExecuteAsync(sessionId, action.PlayerId, action),
                ActionType.Attack => await _attack.ExecuteAsync(sessionId, action.PlayerId, action.CombatAction!),
                ActionType.Block => await _block.ExecuteAsync(sessionId, action.PlayerId, action.CombatAction!),
                ActionType.PassToCombat or ActionType.PreEnd or ActionType.EndTurn
                    => await _passPhase.ExecuteAsync(sessionId, action.PlayerId, action.Type),
                ActionType.CastInstant
                    => await _playCard.ExecuteAsync(sessionId, action.PlayerId, action), // ou méthode dédiée
                ActionType.Discard
                    => await _discard.ExecuteAsync(sessionId, action.PlayerId, action.CardsToDiscard!),
                _ => new ActionResultDto { Success = false, Message = "Action inconnue" }
            };

            if (result.EndGame != null)
                return Ok(result); // ou return StatusCode(410, result); pour signaler la fin de partie

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}