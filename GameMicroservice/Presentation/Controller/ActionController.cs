using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using GameMicroservice.Domain;
using System.Threading.Tasks;

namespace GameMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/games/{gameId}/action")]
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlayAction(
            [FromRoute] string gameId,
            [FromBody] PlayerActionDto action)
        {
            Console.WriteLine($"Received payload: {System.Text.Json.JsonSerializer.Serialize(action)}");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                return BadRequest(new
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = 400,
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }
            if (string.IsNullOrEmpty(gameId))
                return BadRequest("Game id is required");

            if (action == null)
                return BadRequest("Action is required");

            // NOTE: ensure PlayerId is provided (or derive from authenticated user)
            if (string.IsNullOrEmpty(action.PlayerId))
                return BadRequest("PlayerId is required in payload");

            ActionResultDto result = action.Type switch
            {
                ActionType.PlayLand => await _playLand.ExecuteAsync(gameId, action.PlayerId, action.CardId!),
                ActionType.PlayCard => await _playCard.ExecuteAsync(gameId, action.PlayerId, action),
                ActionType.Attack => await _attack.ExecuteAsync(gameId, action.PlayerId, action.CombatAction!),
                ActionType.Block => await _block.ExecuteAsync(gameId, action.PlayerId, action.CombatAction!),
                ActionType.PassToCombat or ActionType.PreEnd or ActionType.EndTurn
                    => await _passPhase.ExecuteAsync(gameId, action.PlayerId, action.Type),
                ActionType.CastInstant
                    => await _playCard.ExecuteAsync(gameId, action.PlayerId, action), // ou méthode dédiée
                ActionType.Discard
                    => await _discard.ExecuteAsync(gameId, action.PlayerId, action.CardsToDiscard!),
                _ => new ActionResultDto { Success = false, Message = "Action inconnue" }
            };

            if (result.EndGame != null)
                return Ok(result); // ou return StatusCode(410, result); pour signaler la fin de partie

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}