using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeckMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/decks")]
    [Authorize]
    public class DecksController : ControllerBase
    {
        private readonly CreateDeckUseCase _create;
        private readonly ValidateDeckUseCase _validate;
        private readonly GetDecksByOwnerUseCase _getDecks;

        public DecksController(
            CreateDeckUseCase create,
            ValidateDeckUseCase validate,
            GetDecksByOwnerUseCase getDecks)
        {
            _create = create;
            _validate = validate;
            _getDecks = getDecks;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeckRequest req)
        {
            // Create the deck after validation
            await _create.ExecuteAsync(req);
            return Ok("Deck created successfully.");
        }

        [HttpGet("{ownerId}")]
        public async Task<IEnumerable<DeckDto>> GetByOwner(string ownerId)
        {
            // Get all decks for an owner
            return await _getDecks.ExecuteAsync(ownerId);
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CreateDeckRequest req)
        {
            // Validate without creating
            var isValid = _validate.Execute(req);
            return Ok(new { IsValid = isValid });
        }
    }
}
