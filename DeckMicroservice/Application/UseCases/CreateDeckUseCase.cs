
using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.UseCases
{
    public class CreateDeckUseCase
    {
        private readonly IDeckRepository _repo;
        private readonly ValidateDeckUseCase _validate;

        public CreateDeckUseCase(IDeckRepository repo, ValidateDeckUseCase validate)
        {
            _repo = repo;
            _validate = validate;
        }

        /// <summary>
        /// Handles the creation of a new deck, including business validation and persistence.
        /// </summary>
        public async Task ExecuteAsync(CreateDeckRequest deck)
        {
            // Business validation: ensures the deck meets all required rules.
            var (isValid, errorMessage) = await _validate.ExecuteAsync(deck);
            if (!isValid)
                throw new InvalidOperationException($"Deck invalide : {errorMessage}");

            // Calls the repository to persist the deck.
            await _repo.CreateAsync(deck);
        }
    }
}