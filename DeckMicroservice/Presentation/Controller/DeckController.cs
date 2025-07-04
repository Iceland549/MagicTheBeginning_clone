using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
            if (req == null)
            {
                Console.WriteLine("Error: Received null CreateDeck request.");
                return BadRequest("Request body is null.");
            }
            Console.WriteLine($"Received CreateDeck request for ownerId: {req.OwnerId}, name: {req.Name}");
            if (req.Cards == null || !req.Cards.Any())
            {
                Console.WriteLine("Warning: No cards provided in CreateDeck request.");
                return BadRequest("No cards provided.");
            }
            Console.WriteLine($"Cards in request: {JsonSerializer.Serialize(req.Cards, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
            await _create.ExecuteAsync(req);
            Console.WriteLine($"Deck created successfully for ownerId: {req.OwnerId}");
            return Ok("Deck created successfully.");
        }

        [HttpGet("{ownerId}")]
        public async Task<IActionResult> GetByOwner(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                Console.WriteLine("Error: Invalid ownerId provided.");
                return BadRequest("OwnerId is required.");
            }
            Console.WriteLine($"Fetching decks for ownerId: {ownerId}");
            var decks = await _getDecks.ExecuteAsync(ownerId);
            if (decks == null)
            {
                Console.WriteLine("No decks found or null result for ownerId.");
                return NotFound("No decks found for this owner.");
            }
            Console.WriteLine($"Retrieved decks: {JsonSerializer.Serialize(decks, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
            return Ok(decks);
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CreateDeckRequest req)
        {
            if (req == null)
            {
                Console.WriteLine("Error: Received null Validate request.");
                return BadRequest("Request body is null.");
            }
            Console.WriteLine($"Received Validate request for ownerId: {req.OwnerId}, name: {req.Name}");
            if (req.Cards == null || !req.Cards.Any())
            {
                Console.WriteLine("Warning: No cards provided in Validate request.");
                return BadRequest("No cards provided.");
            }
            Console.WriteLine($"Cards in request: {JsonSerializer.Serialize(req.Cards, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
            var isValid = _validate.Execute(req);
            Console.WriteLine($"Validation result for deck: {isValid}");
            return Ok(new { IsValid = isValid });
        }
    }
}