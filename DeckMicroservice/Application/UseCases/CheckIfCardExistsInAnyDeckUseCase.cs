using DeckMicroservice.Application.Interfaces;
using System.Threading.Tasks;

namespace DeckMicroservice.Application.UseCases
{
    public class CheckIfCardExistsInAnyDeckUseCase
    {
        private readonly IDeckRepository _repo;

        public CheckIfCardExistsInAnyDeckUseCase(IDeckRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Checks if a specific card (by CardId) is used in any existing deck.
        /// </summary>
        public async Task<bool> ExecuteAsync(string cardId)
        {
            return await _repo.ExistsCardAsync(cardId);
        }
    }
}
