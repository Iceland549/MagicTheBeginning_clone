using DeckMicroservice.Application.DTOs;
using DeckMicroservice.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.UseCases
{
    public class ValidateDeckUseCase
    {
        private readonly IDeckRepository _repo;

        public ValidateDeckUseCase(IDeckRepository repo) => _repo = repo;

        /// <summary>
        /// Validates a deck according to business rules.
        /// </summary>
        /// <returns>A tuple containing the validation result and an error message if applicable.</returns>
        public async Task<(bool IsValid, string ErrorMessage)> ExecuteAsync(CreateDeckRequest deck)
        {
            try
            {
                bool isValid = await _repo.ValidateAsync(deck);
                return (isValid, string.Empty);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error during deck validation: {ex.Message}");
            }
        }
    }
}