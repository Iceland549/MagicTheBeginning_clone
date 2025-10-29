using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases.Combat;
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
        private readonly TapCreatureUseCase _tapCreature;
        private readonly PlayLandUseCase _playLand;
        private readonly PlayCardUseCase _playCard;
        private readonly DeclareAttackersUseCase _declareAttackers;
        private readonly DeclareBlockersUseCase _declareBlockers;
        private readonly ResolveCombatUseCase _resolveCombat;
        private readonly PassPhaseUseCase _passPhase;
        private readonly DrawCardUseCase _drawCard;
        private readonly DiscardUseCase _discard;
        private readonly EndGameUseCase _endGame;
        private readonly PlayerPlayTurnUseCase _playerPlayTurn;

        public ActionController(
            ILogger<ActionController> logger,
            ICardClient cardClient,
            TapLandUseCase tapLand,
            TapCreatureUseCase tapCreature,
            PlayLandUseCase playLand,
            PlayCardUseCase playCard,
            DeclareAttackersUseCase declareAttackers,
            DeclareBlockersUseCase declareBlockers,
            ResolveCombatUseCase resolveCombat,
            PassPhaseUseCase passPhase,
            DrawCardUseCase drawCard,
            DiscardUseCase discard,
            EndGameUseCase endGame,
            PlayerPlayTurnUseCase playerPlayTurn)
        {
            _logger = logger;
            _cardClient = cardClient;
            _tapLand = tapLand;
            _tapCreature = tapCreature;
            _playLand = playLand;
            _playCard = playCard;
            _declareAttackers = declareAttackers;
            _declareBlockers = declareBlockers;
            _resolveCombat = resolveCombat;
            _passPhase = passPhase;
            _drawCard = drawCard;
            _discard = discard;
            _endGame = endGame;
            _playerPlayTurn = playerPlayTurn;
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
                _logger.LogInformation("[DEBUG] Enum parsed value = {ActionTypeValue} ({ActionTypeInt})",
                    action.Type.ToString(), (int)action.Type);
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

                    case ActionType.TapCreature: 
                        {
                            _logger.LogInformation("[ActionController] Handling TapCreature card={CardId} player={PlayerId}",
                                action.CardId, action.PlayerId);

                            if (string.IsNullOrEmpty(action.CardId))
                                return BadRequest("CardId required for TapCreature");

                            var result = await _tapCreature.ExecuteAsync(gameId, action.PlayerId, action.CardId);
                            if (!result.Success)
                                return BadRequest(result.Message);

                            _logger.LogInformation("[ActionController] TapCreature succeeded for card={CardId}",
                                action.CardId);
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

                    case ActionType.PassToCombat:
                        {
                            _logger.LogInformation("[ActionController] Handling PassToCombat for player={PlayerId}", action.PlayerId);

                            var result = await _passPhase.ExecuteAsync(gameId, action.PlayerId, ActionType.PassToCombat);

                            if (!result.Success)
                            {
                                _logger.LogWarning("[ActionController] PassToCombat failed: {Message}", result.Message);
                                return BadRequest(result.Message);
                            }

                            _logger.LogInformation("[ActionController] PassToCombat succeeded");
                            return Ok(result);
                        }

                    case ActionType.DeclareAttackers: 
                        {
                            _logger.LogInformation("[ActionController] Handling DeclareAttackers player={PlayerId} count={Count}",
                                action.PlayerId, action.Attackers?.Count ?? 0);

                            if (action.Attackers == null || !action.Attackers.Any())
                                return BadRequest("Attackers list required");

                            var result = await _declareAttackers.ExecuteAsync(
                                gameId,
                                action.PlayerId,
                                action.Attackers);

                            if (!result.Success)
                                return BadRequest(result.Message);

                            _logger.LogInformation("[ActionController] DeclareAttackers succeeded");
                            return Ok(result);
                        }

                    case ActionType.DeclareBlockers:
                        {
                            _logger.LogInformation("[ActionController] Handling DeclareBlockers player={PlayerId} count={Count}",
                                action.PlayerId, action.Blockers?.Count ?? 0);

                            if (action.Blockers == null)
                                return BadRequest("Blockers dictionary required");

                            var result = await _declareBlockers.ExecuteAsync(
                                gameId,
                                action.PlayerId,
                                action.Blockers);

                            if (!result.Success)
                                return BadRequest(result.Message);

                            _logger.LogInformation("[ActionController] DeclareBlockers succeeded");
                            return Ok(result);
                        }



                    case ActionType.ResolveCombat: 
                        {
                            _logger.LogInformation("[ActionController] Handling ResolveCombat for game={GameId}", gameId);

                            var result = await _resolveCombat.ExecuteAsync(gameId, action.PlayerId);

                            if (!result.Success)
                                return BadRequest(result.Message);

                            if (result.EndGame != null)
                            {
                                _logger.LogInformation("[ActionController] Game ended - Winner={Winner}",
                                    result.EndGame.WinnerId);
                            }

                            _logger.LogInformation("[ActionController] ResolveCombat succeeded");
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
                            var result = await _playerPlayTurn.ExecuteAsync(gameId, action.PlayerId);
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