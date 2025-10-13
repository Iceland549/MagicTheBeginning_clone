using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System;
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
        /// Gère la création d’un nouveau deck, avec validation métier et sauvegarde.
        /// </summary>
        public async Task ExecuteAsync(CreateDeckRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "La requête de création de deck ne peut pas être nulle.");

            // Valide le deck avant sauvegarde
            var (isValid, errorMessage) = await _validate.ExecuteAsync(request);
            if (!isValid)
                throw new InvalidOperationException($"Deck invalide : {errorMessage}");

            // Conversion de CreateDeckRequest en DeckDto pour la persistance
            var deckDto = new DeckDto
            {
                OwnerId = request.OwnerId,
                Name = request.Name,
                Cards = request.Cards
            };

            await _repo.AddAsync(deckDto);
        }
    }
}
