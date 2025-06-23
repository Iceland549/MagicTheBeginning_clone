using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;

namespace DeckMicroservice.Application.UseCases
{
    public class CreateDeckUseCase
    {
        private readonly IDeckRepository _repo;
        public CreateDeckUseCase(IDeckRepository repo) => _repo = repo;

        // Handles the creation of a new deck, including business validation and persistence.
        public async Task ExecuteAsync(CreateDeckRequest deck)
        {
            // Business validation: ensures the deck meets all required rules.
            if (!_repo.Validate(deck))
                throw new InvalidOperationException("Deck invalide : vérifiez le nombre de cartes et de terrains.");

            // Calls the repository to persist the deck.
            await _repo.CreateAsync(deck);
        }
    }
}