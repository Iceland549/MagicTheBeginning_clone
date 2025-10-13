using CardMicroservice.Application.DTOs;
using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Application.UseCases
{
    public class GetCardByNameUseCase
    {
        private readonly ICardRepository _repo;
        private readonly ImportCardUseCase _import;

        public GetCardByNameUseCase(
            ICardRepository repo,
            ImportCardUseCase import)
        {
            _repo = repo;
            _import = import;
        }

        public async Task<CardDto?> ExecuteAsync(string name, string? set = null, string? lang = null, string? collectorNumnber = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var existing = await _repo.GetByNameAsync(name, set, lang, collectorNumnber);
            if (existing != null) return existing;

            var imported = await _import.ExecuteAsync(name, set, lang, collectorNumnber);
            return imported;
        }
    }
}