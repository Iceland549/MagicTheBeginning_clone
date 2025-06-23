using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Application.UseCases
{
    public class GetDecksByOwnerUseCase
    {
        private readonly IDeckRepository _repo;
        public GetDecksByOwnerUseCase(IDeckRepository repo) => _repo = repo;

        // Retrieves all decks belonging to a specific owner.
        public Task<List<DeckDto>> ExecuteAsync(string ownerId) =>
            _repo.GetByOwnerAsync(ownerId);
    }
}