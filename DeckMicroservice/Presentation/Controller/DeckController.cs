using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
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
    [Authorize]
    public class DecksController : ControllerBase
    {
        private readonly ICardClient _cardClient;

        private readonly CreateDeckUseCase _create;
        private readonly ValidateDeckUseCase _validate;
        private readonly GetDecksByOwnerUseCase _getDecks;
        private readonly GetDeckByIdUseCase _getDeckById; 
        private readonly CheckIfCardExistsInAnyDeckUseCase _checkCardExists; 

        public DecksController(
            ICardClient cardClient,
            CreateDeckUseCase create,
            ValidateDeckUseCase validate,
            GetDecksByOwnerUseCase getDecks,
            GetDeckByIdUseCase getDeckById,
            CheckIfCardExistsInAnyDeckUseCase checkCardExists)
        {
            _cardClient = cardClient;
            _create = create;
            _validate = validate;
            _getDecks = getDecks;
            _getDeckById = getDeckById;
            _checkCardExists = checkCardExists;
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
                // Intégration de la validation explicite avant création (inspiré de la refonte)
                var (isValid, errorMessage) = await _validate.ExecuteAsync(req);
                if (!isValid)
                {
                    Console.WriteLine($"Validation failed: {errorMessage}");
                    return BadRequest(new { Error = errorMessage });
                }

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

        [HttpGet("owner/{ownerId}")]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Error: Invalid deck ID provided.");
                return BadRequest(new { Error = "Deck ID is required." });
            }
            try
            {
                var deck = await _getDeckById.ExecuteAsync(id);
                if (deck == null)
                {
                    Console.WriteLine($"No deck found for ID: {id}");
                    return NotFound(new { Error = "Deck not found." });
                }
                return Ok(deck);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching deck by ID: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("exists-card/{cardId}")]
        public async Task<IActionResult> ExistsCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Console.WriteLine("Error: Invalid cardId provided.");
                return BadRequest(new { Error = "CardId is required." });
            }
            try
            {
                var exists = await _checkCardExists.ExecuteAsync(cardId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error checking card existence: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("available-cards")]
        public async Task<IActionResult> GetAvailableCards()
        {
            try
            {
                Console.WriteLine("Fetching all available cards from CardMicroservice...");
                // Appel au CardMicroservice via le client HTTP
                var cards = await _cardClient.GetAllAsync();

                if (cards == null || !cards.Any())
                {
                    Console.WriteLine("No cards found in CardMicroservice.");
                    return NotFound(new { Error = "No cards found in CardMicroservice." });
                }

                Console.WriteLine($"Successfully retrieved {cards.Count()} cards.");
                return Ok(cards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching available cards: {ex}");
                return StatusCode(500, new { Error = $"Internal server error: {ex.Message}" });
            }
        }

    }
}
