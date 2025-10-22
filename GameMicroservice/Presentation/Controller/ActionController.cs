using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GameMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/games/{gameId}/action")]
    public class ActionController : ControllerBase
    {
        private readonly ILogger<ActionController> _logger;
        private readonly ICardClient _cardClient;
        private readonly TapLandUseCase _tapLand;
        private readonly PlayLandUseCase _playLand;
        private readonly PlayCardUseCase _playCard;
        private readonly AttackUseCase _attack;
        private readonly BlockUseCase _block;
        private readonly PassPhaseUseCase _passPhase;
        private readonly DrawCardUseCase _drawCard;
        private readonly DiscardUseCase _discard;
        private readonly EndGameUseCase _endGame;
        private readonly PlayerPlayTurnUseCase _playerPlayTurnUseCase;

        public ActionController(
            ILogger<ActionController> logger,
            ICardClient cardClient,
            TapLandUseCase tapLand,
            PlayLandUseCase playLand,
            PlayCardUseCase playCard,
            AttackUseCase attack,
            BlockUseCase block,
            PassPhaseUseCase passPhase,
            DrawCardUseCase drawCard,
            DiscardUseCase discard,
            EndGameUseCase endGame,
            PlayerPlayTurnUseCase playerPlayTurnUseCase)
        {
            _logger = logger;
            _cardClient = cardClient;
            _tapLand = tapLand;
            _playLand = playLand;
            _playCard = playCard;
            _attack = attack;
            _block = block;
            _passPhase = passPhase;
            _drawCard = drawCard;
            _discard = discard;
            _endGame = endGame;
            _playerPlayTurnUseCase = playerPlayTurnUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlayAction([FromRoute] string gameId, [FromBody] PlayerActionDto action)
        {
            _logger.LogInformation("[ActionController] Payload received for GameId={GameId}: {ActionPayload}", gameId, System.Text.Json.JsonSerializer.Serialize(action));

            if (string.IsNullOrEmpty(gameId) || action == null || string.IsNullOrEmpty(action.PlayerId))
            {
                _logger.LogWarning("[ActionController] Bad request: missing GameId or PlayerId. gameId={GameId} actionNull={ActionNull}", gameId, action == null);
                return BadRequest("Invalid request: missing GameId or PlayerId.");
            }

            try
            {
                switch (action.Type)
                {
                    case ActionType.TapLand:
                        {
                            _logger.LogInformation("[ActionController] Handling TapLand card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                            if (string.IsNullOrEmpty(action.CardId))
                                return BadRequest("CardId required for TapLand");

                            var result = await _tapLand.ExecuteAsync(gameId, action.PlayerId, action.CardId); // Ajoute _tapLand = new TapLandUseCase(...) in constructor
                            if (result.Success == false)
                                return BadRequest(result.Message);

                            _logger.LogInformation("[ActionController] TapLand succeeded for card={CardId}", action.CardId);
                            return Ok(result);
                        }

                    case ActionType.Draw:
                        {
                            _logger.LogInformation("[ActionController] Handling Draw for Player={PlayerId} Game={GameId}", action.PlayerId, gameId);
                            var result = await _drawCard.ExecuteAsync(gameId, action.PlayerId);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] Draw result null for game={GameId} player={PlayerId}", gameId, action.PlayerId);
                                return NotFound();
                            }
                            _logger.LogInformation("[ActionController] Draw succeeded for game={GameId} player={PlayerId}", gameId, action.PlayerId);
                            return Ok(result);
                        }

                    case ActionType.PlayLand:
                        {
                            _logger.LogInformation("[ActionController] Handling PlayLand card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                            if (string.IsNullOrEmpty(action.CardId))
                            {
                                _logger.LogWarning("[ActionController] PlayLand missing CardId for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("CardId is required for PlayLand");
                            }
                            var result = await _playLand.ExecuteAsync(gameId, action.PlayerId, action.CardId);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] PlayLand failed for card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                                return BadRequest("Cannot play land");
                            }
                            _logger.LogInformation("[ActionController] PlayLand succeeded card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                            return Ok(result);
                        }

                    case ActionType.PlayCard:
                        {
                            _logger.LogInformation("[ActionController] Handling PlayCard card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                            var result = await _playCard.ExecuteAsync(gameId, action.PlayerId, action);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] PlayCard failed card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                                return BadRequest("Cannot play card");
                            }
                            _logger.LogInformation("[ActionController] PlayCard succeeded card={CardId} player={PlayerId} game={GameId}", action.CardId, action.PlayerId, gameId);
                            return Ok(result);
                        }

                    case ActionType.Attack:
                        {
                            _logger.LogInformation("[ActionController] Handling Attack player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            if (action.CombatAction == null)
                            {
                                _logger.LogWarning("[ActionController] Attack missing CombatAction for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("CombatAction is required for Attack");
                            }
                            var result = await _attack.ExecuteAsync(gameId, action.PlayerId, action.CombatAction);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] Attack failed for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("Cannot attack");
                            }
                            _logger.LogInformation("[ActionController] Attack succeeded for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            return Ok(result);
                        }

                    case ActionType.Block:
                        {
                            _logger.LogInformation("[ActionController] Handling Block player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            if (action.CombatAction == null)
                            {
                                _logger.LogWarning("[ActionController] Block missing CombatAction for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("CombatAction is required for Block");
                            }
                            var result = await _block.ExecuteAsync(gameId, action.PlayerId, action.CombatAction);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] Block failed for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("Cannot block");
                            }
                            _logger.LogInformation("[ActionController] Block succeeded for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            return Ok(result);
                        }

                    case ActionType.Discard:
                        {
                            _logger.LogInformation("[ActionController] Handling Discard player={PlayerId} game={GameId} cards={Cards}", action.PlayerId, gameId, action.CardsToDiscard);
                            var cards = action.CardsToDiscard ?? new System.Collections.Generic.List<string>();
                            var result = await _discard.ExecuteAsync(gameId, action.PlayerId, cards);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] Discard failed for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return BadRequest("Cannot discard");
                            }
                            _logger.LogInformation("[ActionController] Discard succeeded for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            return Ok(result);
                        }

                    case ActionType.EndTurn:
                        {
                            _logger.LogInformation("[ActionController] Handling EndTurn for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            var result = await _playerPlayTurnUseCase.ExecuteAsync(gameId, action.PlayerId);
                            if (result == null)
                            {
                                _logger.LogWarning("[ActionController] EndTurn result null for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return NotFound("Game session not found or could not process end turn.");
                            }
                            if (result.EndGame != null)
                            {
                                _logger.LogInformation("[ActionController] EndTurn resulted in endgame for game={GameId} winner={Winner}", gameId, result.EndGame.WinnerId);
                                return Ok(result);
                            }
                            if (result.Success)
                            {
                                _logger.LogInformation("[ActionController] EndTurn succeeded for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                                return Ok(result);
                            }
                            _logger.LogWarning("[ActionController] EndTurn returned unsuccessful result for player={PlayerId} game={GameId}", action.PlayerId, gameId);
                            return BadRequest(result);
                        }

                    default:
                        _logger.LogWarning("[ActionController] Unknown or unsupported action type={ActionType} player={PlayerId} game={GameId}", action.Type, action.PlayerId, gameId);
                        return BadRequest("Unknown or unsupported action type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ActionController] Exception while processing PlayAction for game={GameId} player={PlayerId}", gameId, action.PlayerId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erreur interne lors du traitement de l'action",
                    Exception = ex.Message
                });
            }
        }
    }

    [Route("api/games/{gameId}/ai-turn")]
    public class AITurnController : ControllerBase
    {
        private readonly ILogger<AITurnController> _logger;
        private readonly AIPlayTurnUseCase _aiPlayTurn;

        public AITurnController(ILogger<AITurnController> logger, AIPlayTurnUseCase aiPlayTurn)
        {
            _logger = logger;
            _aiPlayTurn = aiPlayTurn;
        }

        [HttpPost]
        public async Task<IActionResult> PlayAITurn([FromRoute] string gameId)
        {
            _logger.LogInformation("[AITurnController] PlayAITurn called for game={GameId}", gameId);
            var result = await _aiPlayTurn.ExecuteAsync(gameId);
            if (result == null)
            {
                _logger.LogWarning("[AITurnController] AIPlayTurn returned null for game={GameId}", gameId);
                return NotFound("Game session not found or AI could not play.");
            }

            _logger.LogInformation("[AITurnController] AIPlayTurn succeeded for game={GameId}", gameId);
            return Ok(result);
        }
    }
}