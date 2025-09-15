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
            if (req == null || string.IsNullOrEmpty(req.PlayerOneId) || string.IsNullOrEmpty(req.PlayerTwoId) || string.IsNullOrEmpty(req.DeckIdP1) || string.IsNullOrEmpty(req.DeckIdP2))
                return BadRequest("Invalid start game request: Player IDs and Deck ID are required");

            var game = await _start.ExecuteAsync(req.PlayerOneId, req.PlayerTwoId, req.DeckIdP1, req.DeckIdP2);
            return Ok(game);
        }

        [HttpPost("{gameId}/ai-turn")]
        public async Task<ActionResult<GameSessionDto>> AITurn(string gameId)
        {
            var result = await _aiTurn.ExecuteAsync(gameId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGameState(string gameId)
        {
            var playerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(playerId))
                return Unauthorized("User not authenticated");

            var dto = await _state.ExecuteAsync(gameId, playerId);
            return Ok(dto);

        }
    }
}