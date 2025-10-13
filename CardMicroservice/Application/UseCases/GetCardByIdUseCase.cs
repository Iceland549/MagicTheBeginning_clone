using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Application.UseCases
{
    /// <summary>
    /// Récupère une carte via son identifiant unique (ScryfallId ou Mongo ObjectId).
    /// </summary>
    public class GetCardByIdUseCase
    {
        private readonly ICardRepository _repo;
        private readonly ImportCardUseCase _import;

        public GetCardByIdUseCase(ICardRepository repo, ImportCardUseCase import)
        {
            _repo = repo;
            _import = import;
        }

        public async Task<CardDto?> ExecuteAsync(string id, string? set = null, string? lang = null, string? collectorNumber = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var card = await _repo.GetByIdAsync(id);
            if (card != null)
                return card;
            var imported = await _import.ExecuteAsync(id, set, lang, collectorNumber);

            if (imported != null)
                Console.WriteLine($"[GetCardByIdUseCase] ✅ Carte importée depuis Scryfall : {imported.Name}");
            else
                Console.WriteLine($"[GetCardByIdUseCase] ❌ Échec de l'import depuis Scryfall pour ID={id}");

            return imported;

        }
    }
}
