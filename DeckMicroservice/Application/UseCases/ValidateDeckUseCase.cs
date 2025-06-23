using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Application.UseCases
{
    public class ValidateDeckUseCase
    {
        private readonly IDeckRepository _repo;
        public ValidateDeckUseCase(IDeckRepository repo) => _repo = repo;

        // Validates a deck according to business rules.
        public bool Execute(CreateDeckRequest deck)
        {
            // Returns true if all business rules are satisfied.
            return _repo.Validate(deck);
        }
    }
}