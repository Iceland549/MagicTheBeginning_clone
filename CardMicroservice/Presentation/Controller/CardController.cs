using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/cards")]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly GetAllCardsUseCase _getAll;
        private readonly GetCardByNameUseCase _getByName;
        private readonly ImportCardUseCase _import;
        private readonly DeleteCardByNameUseCase _deleteByName;


        public CardsController(
            GetAllCardsUseCase getAll,
            GetCardByNameUseCase getByName,
            ImportCardUseCase import,
            DeleteCardByNameUseCase deleteByName)
        {
            _getAll = getAll;
            _getByName = getByName;
            _import = import;
            _deleteByName = deleteByName;
        }
        [HttpGet]
        public async Task<IEnumerable<CardDto>> GetAll()
        {
            // Retrieve all stored cards
            return await _getAll.ExecuteAsync();
        }
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            // Search in database and, if missing, import from Scryfall
            var card = await _getByName.ExecuteAsync(name);
            return card == null ? NotFound() : Ok(card);
        }
        [HttpPost("import/{name}")]
        public async Task<ActionResult<CardDto>> Import(string name)
        {
            // Force import a card from Scryfall
            var card = await _import.ExecuteAsync(name);
            return card == null
                ? NotFound($"Card '{name}' not found on Scryfall")
                : Ok(card);
        }
        [HttpDelete("name/{*name}")]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var deleted = await _deleteByName.ExecuteAsync(name);
            if (!deleted)
                return NotFound($"Carte '{name}' introuvable");
            return NoContent();
        }
    }
}