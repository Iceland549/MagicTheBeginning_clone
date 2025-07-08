using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GameMicroservice.Application.DTOs;
using GameMicroservice.Application.UseCases;

namespace GameMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/games")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly StartGameUseCase _start;
        private readonly PlayCardUseCase _play;
        private readonly GetGameStateUseCase _state;
        private readonly AIPlayTurnUseCase _aiTurn;

        public GameController(
            StartGameUseCase start,
            PlayCardUseCase play,
            GetGameStateUseCase state,
            AIPlayTurnUseCase aiTurn)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _play = play ?? throw new ArgumentNullException(nameof(play));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _aiTurn = aiTurn ?? throw new ArgumentNullException(nameof(aiTurn));
        }

        [HttpPost("start")]
        public async Task<ActionResult<GameSessionDto>> Start([FromBody] StartGameRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.PlayerOneId) || string.IsNullOrEmpty(req.PlayerTwoId) || string.IsNullOrEmpty(req.DeckId))
                return BadRequest("Invalid start game request: Player IDs and Deck ID are required");

            var game = await _start.ExecuteAsync(req.PlayerOneId, req.PlayerTwoId, req.DeckId);
            return Ok(game);
        }

        [HttpPost("{gameId}/action")]
        public async Task<ActionResult<GameSessionDto>> PlayAction(
            string gameId,
            [FromBody] PlayerActionDto action)
        {
            if (string.IsNullOrEmpty(gameId))
                return BadRequest("Game ID is required");
            if (action == null)
                return BadRequest("Action is required");
            if (User.Identity?.Name == null)
                return Unauthorized("User not authenticated");

            var updated = await _play.ExecuteAsync(gameId, User.Identity.Name, action);
            if (updated == null)
                return NotFound($"Game session {gameId} not found");

            return Ok(updated);
        }

        [HttpPost("{gameId}/ai-turn")]
        public async Task<ActionResult<GameSessionDto>> AITurn(string gameId)
        {
            var result = await _aiTurn.ExecuteAsync(gameId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<GameSessionDto>> GetState(string gameId)
        {
            var game = await _state.ExecuteAsync(gameId);
            return Ok(game);
        }
    }
}