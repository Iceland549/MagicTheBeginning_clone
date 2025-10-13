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
        private readonly GetCardByIdUseCase _getById;
        private readonly GetCardByNameUseCase _getByName;
        private readonly ImportCardUseCase _import;
        private readonly DeleteCardByIdUseCase _deleteById;

        public CardsController(
            GetAllCardsUseCase getAll,
            GetCardByIdUseCase getById,
            GetCardByNameUseCase getByName,
            ImportCardUseCase import,
            DeleteCardByIdUseCase deleteById)
        {
            _getAll = getAll;
            _getById = getById;
            _getByName = getByName;
            _import = import;
            _deleteById = deleteById;
        }

        // GET /api/cards
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var cards = await _getAll.ExecuteAsync();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CardController] Erreur : {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
        }   

        // GET /api/cards/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            string id,
            [FromQuery] string? set = null,
            [FromQuery] string? lang = null,
            [FromQuery] string? collectorNumber = null)
        {
            Console.WriteLine($"[CardsController] GetById called id={id} set={set} lang={lang} collector={collectorNumber}");
            var card = await _getById.ExecuteAsync(id, set, lang, collectorNumber);
            return card == null
                ? NotFound($"Carte avec ID '{id}' non trouvée.")
                : Ok(card);
        }

        // GET /api/cards/name/{name}?set=...&lang=...&collectorNumber=...
        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name, [FromQuery] string? set = null, [FromQuery] string? lang = null, [FromQuery] string? collectorNumber = null)
        {
            var card = await _getByName.ExecuteAsync(name, set, lang, collectorNumber);
            return card == null
                ? NotFound($"Carte '{name}' non trouvée.")
                : Ok(card);
        }

        // POST /api/cards/import/{name}
        [HttpPost("import/{name}")]
        public async Task<ActionResult<CardDto>> Import(string name, [FromQuery] string? set = null, [FromQuery] string? lang = null, [FromQuery] string? collectorNumber = null)
        {
            var card = await _import.ExecuteAsync(name, set, lang, collectorNumber);
            return card == null
                ? NotFound($"Carte '{name}' non trouvée sur Scryfall.")
                : Ok(card);
        }

        // DELETE /api/cards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(string id)
        {
            var deleted = await _deleteById.ExecuteAsync(id);
            if (!deleted)
                return NotFound($"Carte avec ID '{id}' introuvable.");
            return NoContent();
        }
    }
}
