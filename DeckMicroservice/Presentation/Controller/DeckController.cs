using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeckMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/decks")]
    //[Authorize]
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
                return BadRequest(new { Error = "Request body is null." });
            }
            Console.WriteLine($"Received CreateDeck request for ownerId: {req.OwnerId}, name: {req.Name}");
            if (req.Cards == null || !req.Cards.Any())
            {
                Console.WriteLine("Warning: No cards provided in CreateDeck request.");
                return BadRequest(new { Error = "No cards provided." });
            }
            Console.WriteLine($"Cards in request: {JsonSerializer.Serialize(req.Cards, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
            try
            {
                await _create.ExecuteAsync(req);
                Console.WriteLine($"Deck created successfully for ownerId: {req.OwnerId}");
                return Ok(new { Message = "Deck created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error creating deck: {ex.Message}");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error creating deck: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{ownerId}")]
        public async Task<IActionResult> GetByOwner(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                Console.WriteLine("Error: Invalid ownerId provided.");
                return BadRequest(new { Error = "OwnerId is required." });
            }
            Console.WriteLine($"Fetching decks for ownerId: {ownerId}");
            try
            {
                var decks = await _getDecks.ExecuteAsync(ownerId);
                if (decks == null)
                {
                    Console.WriteLine("No decks found or null result for ownerId.");
                    return NotFound(new { Error = "No decks found for this owner." });
                }
                Console.WriteLine($"Retrieved decks: {JsonSerializer.Serialize(decks, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
                return Ok(decks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching decks: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] CreateDeckRequest req)
        {
            if (req == null)
            {
                Console.WriteLine("Error: Received null Validate request.");
                return BadRequest(new { Error = "Request body is null." });
            }
            Console.WriteLine($"Received Validate request for ownerId: {req.OwnerId}, name: {req.Name}");
            if (req.Cards == null || !req.Cards.Any())
            {
                Console.WriteLine("Warning: No cards provided in Validate request.");
                return BadRequest(new { Error = "No cards provided." });
            }
            Console.WriteLine($"Cards in request: {JsonSerializer.Serialize(req.Cards, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })}");
            try
            {
                var (isValid, errorMessage) = await _validate.ExecuteAsync(req);
                Console.WriteLine($"Validation result for deck: IsValid={isValid}, ErrorMessage={errorMessage}");
                return Ok(new { IsValid = isValid, ErrorMessage = errorMessage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error validating deck: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("all")]
        public async Task<ActionResult<List<DeckDto>>> GetAllDecks()
        {
            var decks = await _getDecks.ExecuteAllAsync();
            return Ok(decks);
        }

    }
}