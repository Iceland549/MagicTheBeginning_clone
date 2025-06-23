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

        public async Task<CardDto?> ExecuteAsync(string name)
        {
            // Si existe, renvoie tout de suite
            var existing = await _repo.GetByNameAsync(name);
            if (existing != null) return existing;

            // Sinon, importe depuis Scryfall
            return await _import.ExecuteAsync(name);
        }
    }
}